using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation
{
    public class HekateScriptVisitor : HekateBaseVisitor<CodeBlock>
    {
        private readonly IVirtualMachine _virtualMachine;
        private readonly IScopeManager _scopeManager;

        public HekateScriptVisitor(IVirtualMachine virtualMachine, IScopeManager scopeManager)
        {
            _virtualMachine = virtualMachine;
            _scopeManager = scopeManager;
        }

        #region Top-level constructs


        public override CodeBlock VisitScript(HekateParser.ScriptContext context)
        {
            foreach (var child in context.children)
            {
                // visit each child and append the code to the main record
                var childRecord = Visit(child);
                if(childRecord == null) throw new InvalidOperationException("A visit to a child resulted in a null return value; check the visitor and make sure it overrides " + child.GetType().Name + "\'s visit method.");
            }

            return null; // TODO: WHAT THE FUCK?
        }

        public override CodeBlock VisitEmitterUpdaterDeclaration(HekateParser.EmitterUpdaterDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();

            var paramNames = paramContexts.Select(fpc => fpc.NormalIdentifier().GetText());
            var name = context.NormalIdentifier().GetText();


            var eUpdaterCodeBlock = new EmitterUpdaterCodeBlock(paramNames);
            _scopeManager.Add(eUpdaterCodeBlock);
            foreach (var child in context.children) {
                eUpdaterCodeBlock.Add(Visit(child));
            }
            _scopeManager.Remove();

            // done, now add to the pool of emitter updater records
            _virtualMachine.AddEmitterUpdaterCodeBlock(name, eUpdaterCodeBlock);
            
            return eUpdaterCodeBlock;
        }

        public override CodeBlock VisitBulletUpdaterDeclaration(HekateParser.BulletUpdaterDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();

            var paramNames = paramContexts.Select(fpc => fpc.NormalIdentifier().GetText());
            var name = context.NormalIdentifier().GetText();


            var bUpdaterCodeBlock = new BulletUpdaterCodeBlock(paramNames);
            _scopeManager.Add(bUpdaterCodeBlock);
            foreach (var child in context.children)
            {
                bUpdaterCodeBlock.Add(Visit(child));
            }
            _scopeManager.Remove();

            // done, now add to the pool of bullet updater records
            _virtualMachine.AddBulletUpdaterCodeBlock(name, bUpdaterCodeBlock);

            return bUpdaterCodeBlock;
        }

        public override CodeBlock VisitFunctionDeclaration(HekateParser.FunctionDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();

            var paramNames = paramContexts.Select(fpc => fpc.NormalIdentifier().GetText());
            var name = context.NormalIdentifier().GetText();


            var funcCodeBlock = new FunctionCodeBlock(paramNames);
            _scopeManager.Add(funcCodeBlock);
            foreach (var child in context.children)
            {
                funcCodeBlock.Add(Visit(child));
            }
            _scopeManager.Remove();

            // done, now add to the pool of function records
            _virtualMachine.AddFunctionCodeBlock(name, funcCodeBlock);

            return funcCodeBlock;
        }


        #endregion


        #region Statement constructs
        public override CodeBlock VisitExpressionStatement(HekateParser.ExpressionStatementContext context)
        {
            return Visit(context.expression());
            // TODO: pop after expression statement
        }

        public override CodeBlock VisitVariableDeclaration(HekateParser.VariableDeclarationContext context)
        {
            var code = new CodeBlock();
            var scope = _scopeManager.GetCurrentScope();

            var variableName = context.NormalIdentifier().GetText();
            var index = scope.AddNumericalVariable(variableName);

            // NOTE: this declaration only happens for numeral assignments
            // Variable declaration code:
            // {evaluate expression, should place value on stack}
            // Instruction.SetVariable
            // {index of the variable}

            code.Add(Visit(context.expression()));
            code.Add(Instruction.SetVariable);
            code.Add(index);

            return code;
        }

        public override CodeBlock VisitReturnStatement(HekateParser.ReturnStatementContext context)
        {
            var code = new CodeBlock();

            // Return statement code:
            // Generate code for expression
            // Instruction.Return

            code.Add(Visit(context.expression()));
            code.Add(Instruction.Return);

            return code;
        }

        public override CodeBlock VisitIfStatement(HekateParser.IfStatementContext context)
        {
            var code = new CodeBlock();
            var ifBodyStatement = context.statement(0);
            var elseStatement = context.statement(1);

            // If statement code
            // Generate parExpression code
            // Instruction.JumpIfZero
            // The offset to jump if the parExpression is 0 (size of statement)
            // Generate expression code for statement
            // Instruction.Jump if needed
            // offset to jump if needed
            // Generate code for statement (if it exists)

            code.Add(Visit(context.parExpression()));
            code.Add(Instruction.JumpIfZero);

            var ifBodyCode = Visit(ifBodyStatement);

            if (elseStatement != null) {
                code.Add(ifBodyCode.Size + 2); // + 2 because of the jump statement
                code.Add(ifBodyCode);

                var elseCode = Visit(elseStatement);
                code.Add(Instruction.Jump);
                code.Add(elseCode.Size);
                code.Add(elseCode);
            }
            else {
                code.Add(ifBodyCode.Size);
                code.Add(ifBodyCode);
            }

            return code;
        }

        public override CodeBlock VisitWhileStatement(HekateParser.WhileStatementContext context)
        {
            var code = new CodeBlock();
            var whileBodyStatement = context.statement();

            // While statement code
            // Generate code for parExpression
            // Instruction.JumpIfZero
            // The offset to jump if the parExpression is 0 (size of statement)
            // Generate expression code for statement
            // Instruction.Jump
            // offset to jump (should be negative, from the jump instruction back to the beginning of the codeblock

            code.Add(Visit(context.parExpression()));
            code.Add(Instruction.JumpIfZero);

            var bodyCode = Visit(whileBodyStatement);
            code.Add(bodyCode.Size + 2); // + 2 because of the jump statement
            code.Add(bodyCode);

            var numInstructionsToJumpBack = code.Size;
            code.Add(Instruction.Jump);
            code.Add(-numInstructionsToJumpBack); // negative because this is a jump backwards

            return code;
        }

        public override CodeBlock VisitForStatement(HekateParser.ForStatementContext context)
        {
            var code = new CodeBlock();
            var forInitCtx = context.forControl().forInit();
            var expressionCtx = context.forControl().expression();
            var forUpdateCtx = context.forControl().forUpdate();
            var bodyStatement = context.statement();

            // For statement code
            // Generate code for the initialization (if there is one)
            // Generate code for the test expression (if there is one)
            // Generate code for the increment expressions (if there are any)
            // Generate code for the body statement
            // Instruction.Jump
            // the number of instructions backwards until we get to the test expression

            if (forInitCtx != null) code.Add(Visit(forInitCtx));

            var codeToLoop = new CodeBlock();
            if (expressionCtx != null) codeToLoop.Add(Visit(expressionCtx));
            if (forUpdateCtx != null) codeToLoop.Add(Visit(forUpdateCtx));
            codeToLoop.Add(Visit(bodyStatement));

            code.Add(codeToLoop);

            var numInstructionsToJumpBack = codeToLoop.Size;
            code.Add(Instruction.Jump);
            code.Add(-numInstructionsToJumpBack); // negative because this is a jump backwards

            return code;
        }

        #endregion

        #region Miscellaneous statements (usually ones that wrap around expressions)
        
        public override CodeBlock VisitParenthesizedExpression(HekateParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expression());
        }

        public override CodeBlock VisitParExpression(HekateParser.ParExpressionContext context)
        {
            return Visit(context.expression());
        }

        public override CodeBlock VisitParExpressionList(HekateParser.ParExpressionListContext context)
        {
            return context.expressionList() == null ? new CodeBlock() : Visit(context.expressionList());
        }

        public override CodeBlock VisitExpressionList(HekateParser.ExpressionListContext context)
        {
            var code = new CodeBlock();
            
            foreach (var expressionContext in context.expression()) {
                code.Add(Visit(expressionContext));
            }

            return code;
        }

        public override CodeBlock VisitBlockStatement(HekateParser.BlockStatementContext context)
        {
            return Visit(context.block());
        }

        public override CodeBlock VisitBlock(HekateParser.BlockContext context)
        {
            var code = new CodeBlock();

            foreach (var statementContext in context.statement()) {
                code.Add(Visit(statementContext));
            }

            return code;
        }

        public override CodeBlock VisitForInit(HekateParser.ForInitContext context)
        {
            var isVariableDeclaration = context.variableDeclaration() != null;

            return isVariableDeclaration ? Visit(context.variableDeclaration()) : Visit(context.expressionList());
        }

        #endregion


        #region Expression constructs

        public override CodeBlock VisitLiteralExpression(HekateParser.LiteralExpressionContext context)
        {
            var code = new CodeBlock();
            var text = context.GetText();
            var value = float.Parse(text);

            code.Add(Instruction.Push);
            code.Add(value);

            return code;
        }

        public override CodeBlock VisitNormalIdentifierExpression(HekateParser.NormalIdentifierExpressionContext context)
        {
            // Normal identifier expression code:
            // Instructions.Push
            // {index of variable if it exists}
            return GenerateCodeForValueOfVariable(context.NormalIdentifier().GetText());
        }

        public override CodeBlock VisitPropertyIdentifierExpression(HekateParser.PropertyIdentifierExpressionContext context)
        {
            // Property identifier expression code:
            // Instructions.Push
            // {index of property if it exists}
            return GenerateCodeForValueOfProperty(context.PropertyIdentifier().GetText());
        }

        public override CodeBlock VisitPostIncDecExpression(HekateParser.PostIncDecExpressionContext context)
        {
            var code = new CodeBlock();

            // TODO: heavily reuses code from VisitAssignmentExpression - figure out a way to de-duplicate code

            var isNormalIdentifier = context.NormalIdentifier() != null;
            var isPropertyIdentifier = context.PropertyIdentifier() != null;

            // Post-inc/decrement expression code:
            // Instruction.GetVariable/Property
            // {index of variable/property}
            // Instructions.Push
            // {1}
            // Instruction.OperatorAdd/Subtract
            // Instruction.SetVariable/Property
            // {index of variable/property}

            // determine whether its a variable or a property
            int index;
            Instruction assignmentOp;
            CodeBlock codeForValueOfLeftSide; // to make sure redundant checks arent done later on)
            if (isNormalIdentifier)
            {
                var scope = _scopeManager.GetCurrentScope();
                var identifierName = context.NormalIdentifier().GetText();
                index = scope.GetNumericalVariable(identifierName).Index;
                assignmentOp = Instruction.SetVariable;

                codeForValueOfLeftSide = GenerateCodeForValueOfVariable(identifierName);
            }
            else if (isPropertyIdentifier)
            {
                var identifierName = context.PropertyIdentifier().GetText();
                index = _virtualMachine.GetProperty(identifierName).Index;
                assignmentOp = Instruction.SetProperty;

                codeForValueOfLeftSide = GenerateCodeForValueOfProperty(identifierName);
            }
            else
            {
                throw new InvalidOperationException("You forgot to add a case for another identifier type! Check the code for VisitPostIncDecExpression.");
            }

            code.Add(codeForValueOfLeftSide);
            code.Add(Instruction.Push);
            code.Add(1);

            Instruction instToAdd;
            switch (context.Operator.Type) {
                case HekateParser.INC:  instToAdd = Instruction.OperatorAdd; break;
                case HekateParser.DEC:  instToAdd = Instruction.OperatorSubtract; break;
                default:
                    throw new InvalidOperationException("The post-increment/decrement expression had an operator that was neither increment nor decrement. Check VisitPostIncDecExpression code.");
            }

            code.Add(instToAdd);
            code.Add(assignmentOp);
            code.Add(index);

            return code;
        }

        public override CodeBlock VisitAssignmentExpression(HekateParser.AssignmentExpressionContext context)
        {
            var code = new CodeBlock();

            var isNormalIdentifier = context.NormalIdentifier() != null;
            var isPropertyIdentifier = context.PropertyIdentifier() != null;

            // NOTE: this assignment only happens for numeral assignments
            // Assignment expression code:
            // 1a. if normal assign
            //  {evaluate expression, should place value on stack}
            // 1b. if mul/div/add/sub
            //  i.   {evaluate identifier's value, should place value on stack}
            //  ii.  {evaluate expression, should place value on stack}
            //  iii. {an Instruction depending on what kind of assignment}
            // 2. Instruction.SetVariable or Instruction.SetProperty
            // 3. {index of the variable}


            // determine whether its a variable or a property
            int index;
            Instruction assignmentOp;
            CodeBlock codeToAdd; // to make sure redundant checks arent done later on)
            if (isNormalIdentifier)
            {
                var scope = _scopeManager.GetCurrentScope();
                var identifierName = context.NormalIdentifier().GetText();
                index = scope.GetNumericalVariable(identifierName).Index;
                assignmentOp = Instruction.SetVariable;

                codeToAdd = GenerateCodeForValueOfVariable(identifierName);
            }
            else if (isPropertyIdentifier)
            {
                var identifierName = context.PropertyIdentifier().GetText();
                index = _virtualMachine.GetProperty(identifierName).Index;
                assignmentOp = Instruction.SetProperty;

                codeToAdd = GenerateCodeForValueOfProperty(identifierName);
            }
            else
            {
                throw new InvalidOperationException("You forgot to add a case for another identifier type! Check the code for VisitAssignmentExpression.");
            }

            // 1
            if (context.Operator.Type == HekateParser.ASSIGN) { // a
                code.Add(Visit(context.expression()));
            }
            else { // b
                code.Add(codeToAdd); // i
                code.Add(Visit(context.expression())); // ii
                code.Add(GetCompoundAssignmentOperatorFromContext(context)); // iii
            }

            // 2,3
            code.Add(assignmentOp);
            code.Add(index);

            return code;
        }

        public override CodeBlock VisitFunctionCallExpression(HekateParser.FunctionCallExpressionContext context)
        {
            var code = new CodeBlock();

            var functionName = context.NormalIdentifier().GetText();
            var functionIndex = _virtualMachine.GetFunctionCodeBlock(functionName).Index;

            // Function call expression code:
            // Generate code for each parameter value (each should push a value onto the stack)
            // Instruction.FunctionCall
            // {function code block's index}

            code.Add(Visit(context.parExpressionList()));
            code.Add(Instruction.FunctionCall);
            code.Add(functionIndex);

            return code;
        }

        public override CodeBlock VisitUnaryExpression(HekateParser.UnaryExpressionContext context)
        {
            var code = new CodeBlock();

            // Unary expression code:
            // Generate code for expression (should push onto stack)
            // Instruction.{depends on context.Operator.Type}
            code.Add(Visit(context.expression()));
            code.Add(GetUnaryOperatorFromContext(context));

            return code;
        }

        public override CodeBlock VisitBinaryExpression(HekateParser.BinaryExpressionContext context)
        {
            var code = new CodeBlock();

            // Binary expression code:
            // Generate code for left expression (should push onto stack)
            // Generate code for right expression (should push onto stack)
            // Instruction.{depends on context.Operator.Type}

            code.Add(Visit(context.expression(0)));
            code.Add(Visit(context.expression(1)));
            code.Add(GetBinaryOperatorFromContext(context));

            return code;
        }

        private Instruction GetUnaryOperatorFromContext(HekateParser.UnaryExpressionContext context)
        {
            switch (context.Operator.Type)
            {
                case HekateParser.SUB:      return Instruction.Negate;
                case HekateParser.BANG:     return Instruction.OperatorNot;
                default:                    throw new InvalidOperationException("You forgot to add support for an operator! Check the code for support for the " + context.Operator.Text + " operator.");
            }
        }

        private Instruction GetBinaryOperatorFromContext(HekateParser.BinaryExpressionContext context)
        {
            switch (context.Operator.Type)
            {
                case HekateParser.MUL:      return Instruction.OperatorMultiply;
                case HekateParser.DIV:      return Instruction.OperatorDivide;
                case HekateParser.MOD:      return Instruction.OperatorMod;
                case HekateParser.ADD:      return Instruction.OperatorAdd;
                case HekateParser.SUB:      return Instruction.OperatorSubtract;
                case HekateParser.LT:       return Instruction.OperatorLessThan;
                case HekateParser.GT:       return Instruction.OperatorGreaterThan;
                case HekateParser.LE:       return Instruction.OperatorLessThanEqual;
                case HekateParser.GE:       return Instruction.OperatorGreaterThanEqual;
                case HekateParser.EQUAL:    return Instruction.OperatorEqual;
                case HekateParser.NOTEQUAL: return Instruction.OperatorNotEqual;
                case HekateParser.AND:      return Instruction.OperatorAnd;
                case HekateParser.OR:       return Instruction.OperatorOr;
                default:                    throw new InvalidOperationException("You forgot to add support for an operator! Check the code for support for the " + context.Operator.Text + " operator.");
            }
        }

        private Instruction GetCompoundAssignmentOperatorFromContext(HekateParser.AssignmentExpressionContext context)
        {
            switch (context.Operator.Type)
            {
                case HekateParser.MUL_ASSIGN:   return Instruction.OperatorMultiply;
                case HekateParser.DIV_ASSIGN:   return Instruction.OperatorDivide;
                case HekateParser.ADD_ASSIGN:   return Instruction.OperatorAdd;
                case HekateParser.SUB_ASSIGN:   return Instruction.OperatorSubtract;
                default:
                    throw new InvalidOperationException(
                        "You forgot to add a case for a compound assignment operator! Check the code for GetCompoundAssignmentOperatorFromContext.");
            }
        }

        #endregion

        private CodeBlock GenerateCodeForValueOfVariable(string name)
        {
            var scope = _scopeManager.GetCurrentScope();
            var index = scope.GetNumericalVariable(name).Index;

            var code = new CodeBlock();
            code.Add(Instruction.GetVariable);
            code.Add(index);

            return code;
        }

        private CodeBlock GenerateCodeForValueOfProperty(string name)
        {
            var index = _virtualMachine.GetProperty(name).Index;

            var code = new CodeBlock();
            code.Add(Instruction.GetProperty);
            code.Add(index);

            return code;
        }
    }
}
