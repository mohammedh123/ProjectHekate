using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using ProjectHekate.Scripting;
using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Bytecode.Generators;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Grammar.Implementation
{
    public class HekateScriptVisitor : HekateBaseVisitor<AbstractBytecodeEmitter>
    {
        private readonly IVirtualMachine _virtualMachine;
        private readonly IScopeManager _scopeManager;

        // These two stack's of lists are used to replace the dummy values for jumps/continues
        //  with the actual size of the enclosing loop constructs they should be in
        // When a loop construct 'begins', it should add an empty list to each of these
        // When a loop construct completes adding its code in its entirety, it should:
        //  1. Pop the top of the stack
        //  2. Iterate over each list and replace code[location] with the appropriate jump offset
        private readonly Stack<List<uint>> _breakLocations;
        private readonly Stack<List<uint>> _continueLocations;

        public HekateScriptVisitor(IVirtualMachine virtualMachine, IScopeManager scopeManager)
        {
            _virtualMachine = virtualMachine;
            _scopeManager = scopeManager;

            _breakLocations = new Stack<List<uint>>();
            _continueLocations = new Stack<List<uint>>();
        }

        #region Top-level constructs

        public override AbstractBytecodeEmitter VisitScript(HekateParser.ScriptContext context)
        {
            foreach (var child in context.children)
            {
                // visit each child and append the code to the main record
                var childRecord = Visit(child);
                if(childRecord == null) throw new InvalidOperationException("A visit to a child resulted in a null return value; check the visitor and make sure it overrides " + child.GetType().Name + "\'s visit method.");
            }

            return null; // TODO: WHAT THE FUCK?
        }

        private void AddNewScope(CodeScope codeScope)
        {
            _scopeManager.Add(codeScope);
            _virtualMachine.CurrentCode = codeScope;
        }

        private void RemoveMostRecentScope()
        {
            _scopeManager.Remove();
            _virtualMachine.CurrentCode = null;
        }

        public override AbstractBytecodeEmitter VisitEmitterUpdaterDeclaration(HekateParser.EmitterUpdaterDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();
            var paramNames = paramContexts.Select(fpc => fpc.NormalIdentifier().GetText());
            var name = context.NormalIdentifier().GetText();
            var eUpdaterCodeBlock = new EmitterUpdaterCodeScope(paramNames);

            AddNewScope(eUpdaterCodeBlock);
            foreach (var child in context.children) {
                Visit(child);
            }
            RemoveMostRecentScope();

            // done, now add to the pool of emitter updater records
            _virtualMachine.AddEmitterUpdaterCodeScope(name, eUpdaterCodeBlock);
            
            return eUpdaterCodeBlock;
        }

        public override AbstractBytecodeEmitter VisitBulletUpdaterDeclaration(HekateParser.BulletUpdaterDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();
            var paramNames = paramContexts.Select(fpc => fpc.NormalIdentifier().GetText());
            var name = context.NormalIdentifier().GetText();
            var bUpdaterCodeBlock = new BulletUpdaterCodeScope(paramNames);

            AddNewScope(bUpdaterCodeBlock);
            foreach (var child in context.children) {
                Visit(child);
            }
            RemoveMostRecentScope();

            // done, now add to the pool of bullet updater records
            _virtualMachine.AddBulletUpdaterCodeScope(name, bUpdaterCodeBlock);

            return bUpdaterCodeBlock;
        }

        public override AbstractBytecodeEmitter VisitFunctionDeclaration(HekateParser.FunctionDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();
            var paramNames = paramContexts.Select(fpc => fpc.NormalIdentifier().GetText());
            var name = context.NormalIdentifier().GetText();
            var funcCodeBlock = new FunctionCodeScope(paramNames);

            AddNewScope(funcCodeBlock);
            foreach (var child in context.children) {
                Visit(child);
            }
            RemoveMostRecentScope();

            // done, now add to the pool of function records
            _virtualMachine.AddFunctionCodeScope(name, funcCodeBlock);

            return funcCodeBlock;
        }

        #endregion
      
  
        #region Statement constructs

        public override AbstractBytecodeEmitter VisitExpressionStatement(HekateParser.ExpressionStatementContext context)
        {
            var expGen = Visit(context.expression()); // all expressions should leave a single value on the stack
            var expStatementEmitter = new ExpressionStatementEmitter(expGen);

            return expStatementEmitter;
        }

        public override AbstractBytecodeEmitter VisitVariableDeclaration(HekateParser.VariableDeclarationContext context)
        {
            var code = new CodeScope();
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

        public override AbstractBytecodeEmitter VisitReturnStatement(HekateParser.ReturnStatementContext context)
        {
            var code = new CodeScope();

            // Return statement code:
            // Generate code for expression
            // Instruction.Return

            code.Add(Visit(context.expression()));
            code.Add(Instruction.Return);

            return code;
        }

        public override AbstractBytecodeEmitter VisitIfStatement(HekateParser.IfStatementContext context)
        {
            var code = new CodeScope();
            var ifBodyStatement = context.statement(0);
            var elseStatement = context.statement(1);

            // If statement code
            // Generate parExpression code
            // Instruction.JumpOffsetIfZero
            // The offset to jump if the parExpression is 0 (size of statement)
            // Generate expression code for statement
            // Instruction.JumpOffset if needed
            // offset to jump if needed
            // Generate code for statement (if it exists)

            code.Add(Visit(context.parExpression()));
            code.Add(Instruction.JumpOffsetIfZero);

            var ifBodyCode = Visit(ifBodyStatement);

            if (elseStatement != null) {
                code.Add(ifBodyCode.Size + 2); // + 2 because of the jump statement
                code.Add(ifBodyCode);

                var elseCode = Visit(elseStatement);
                code.Add(Instruction.JumpOffset);
                code.Add(elseCode.Size);
                code.Add(elseCode);
            }
            else {
                code.Add(ifBodyCode.Size);
                code.Add(ifBodyCode);
            }

            return code;
        }

        public override AbstractBytecodeEmitter VisitForStatement(HekateParser.ForStatementContext context)
        {
            var code = new CodeScope();
            var forInitCtx = context.forControl().forInit();
            var expressionCtx = context.forControl().expression();
            var forUpdateCtx = context.forControl().forUpdate();
            var bodyStatement = context.statement();

            // For statement code
            // Generate code for the initialization (if there is one)
            // Generate code for the test expression (if there is one)
            // Generate code for the increment expressions (if there are any)
            // Generate code for the body statement
            // Instruction.JumpOffset
            // the number of instructions backwards until we get to the test expression

            if (forInitCtx != null) code.Add(Visit(forInitCtx));

            var codeToLoop = new CodeScope();
            if (expressionCtx != null) codeToLoop.Add(Visit(expressionCtx));
            if (forUpdateCtx != null) codeToLoop.Add(Visit(forUpdateCtx));
            codeToLoop.Add(Visit(bodyStatement));

            code.Add(codeToLoop);

            var numInstructionsToJumpBack = codeToLoop.Size;
            code.Add(Instruction.JumpOffset);
            code.Add(-numInstructionsToJumpBack); // negative because this is a jump backwards

            return code;
        }

        public override AbstractBytecodeEmitter VisitWhileStatement(HekateParser.WhileStatementContext context)
        {
            var code = new CodeScope();
            var whileBodyStatement = context.statement();

            _breakLocations.Push(new List<uint>());
            _continueLocations.Push(new List<uint>());

            // While statement code
            // Generate code for parExpression
            // Instruction.JumpOffsetIfZero
            // The offset to jump if the parExpression is 0 (size of statement)
            // Generate expression code for statement
            // Instruction.JumpOffset
            // offset to jump (should be negative, from the jump instruction back to the beginning of the codeblock

            code.Add(Visit(context.parExpression()));
            code.Add(Instruction.JumpOffsetIfZero);

            var bodyCode = Visit(whileBodyStatement);
            code.Add(bodyCode.Size + 2); // + 2 because of the jump statement
            code.Add(bodyCode);

            var numInstructionsToJumpBack = code.Size;
            code.Add(Instruction.JumpOffset);
            code.Add(-numInstructionsToJumpBack); // negative because this is a jump backwards
            
            return code;
        }

        public override AbstractBytecodeEmitter VisitBreakStatement(HekateParser.BreakStatementContext context)
        {
            var code = new CodeScope();

            // Break statement
            // Instruction.JumpOffset
            // {0}, used as a dummy value; the enclosing loop construct must take care of replacing 
            //      the dummy value with the actual size of the construct's code scope
            // add a break to the list of breakStatements in the visitor

            code.Add(Instruction.JumpOffset);
            code.Add((byte)0);

            //_breakLocations.Peek().Add();

            return code;
        }

        #endregion


        #region Miscellaneous statements (usually ones that wrap around expressions)
        
        public override AbstractBytecodeEmitter VisitParenthesizedExpression(HekateParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expression());
        }

        public override AbstractBytecodeEmitter VisitParExpression(HekateParser.ParExpressionContext context)
        {
            return Visit(context.expression());
        }

        public override AbstractBytecodeEmitter VisitParExpressionList(HekateParser.ParExpressionListContext context)
        {
            return context.expressionList() == null ? new CodeScope() : Visit(context.expressionList());
        }

        public override AbstractBytecodeEmitter VisitExpressionList(HekateParser.ExpressionListContext context)
        {
            var code = new CodeScope();
            
            foreach (var expressionContext in context.expression()) {
                code.Add(Visit(expressionContext));
            }

            return code;
        }

        public override AbstractBytecodeEmitter VisitBlockStatement(HekateParser.BlockStatementContext context)
        {
            return Visit(context.block());
        }

        public override AbstractBytecodeEmitter VisitBlock(HekateParser.BlockContext context)
        {
            var code = new CodeScope();

            foreach (var statementContext in context.statement()) {
                code.Add(Visit(statementContext));
            }

            return code;
        }

        public override AbstractBytecodeEmitter VisitForInit(HekateParser.ForInitContext context)
        {
            var isVariableDeclaration = context.variableDeclaration() != null;

            return isVariableDeclaration ? Visit(context.variableDeclaration()) : Visit(context.expressionList());
        }

        #endregion


        #region Expression constructs

        public override AbstractBytecodeEmitter VisitLiteralExpression(HekateParser.LiteralExpressionContext context)
        {
            var text = context.GetText();
            var value = float.Parse(text);

            return new LiteralExpressionGenerator(value);
        }

        public override AbstractBytecodeEmitter VisitNormalIdentifierExpression(HekateParser.NormalIdentifierExpressionContext context)
        {
            var identifierName = context.NormalIdentifier().GetText();

            return new NormalIdentifierExpressionGenerator(identifierName);
        }

        public override AbstractBytecodeEmitter VisitPropertyIdentifierExpression(HekateParser.PropertyIdentifierExpressionContext context)
        {
            var identifierName = context.PropertyIdentifier().GetText();

            return new PropertyIdentifierExpressionGenerator(identifierName);
        }

        public override AbstractBytecodeEmitter VisitPostIncDecExpression(HekateParser.PostIncDecExpressionContext context)
        {
            var code = new CodeScope();

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
            CodeScope codeForValueOfLeftSide; // to make sure redundant checks arent done later on)
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

        public override AbstractBytecodeEmitter VisitAssignmentExpression(HekateParser.AssignmentExpressionContext context)
        {
            var code = new CodeScope();

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
            CodeScope codeToAdd; // to make sure redundant checks arent done later on)
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

        public override AbstractBytecodeEmitter VisitFunctionCallExpression(HekateParser.FunctionCallExpressionContext context)
        {
            var code = new CodeScope();

            var functionName = context.NormalIdentifier().GetText();
            var functionIndex = _virtualMachine.GetFunctionCodeScope(functionName).Index;

            // Function call expression code:
            // Generate code for each parameter value (each should push a value onto the stack)
            // Instruction.FunctionCall
            // {function code scope's index}

            code.Add(Visit(context.parExpressionList()));
            code.Add(Instruction.FunctionCall);
            code.Add(functionIndex);

            return code;
        }

        public override AbstractBytecodeEmitter VisitUnaryExpression(HekateParser.UnaryExpressionContext context)
        {
            var code = new CodeScope();

            // Unary expression code:
            // Generate code for expression (should push onto stack)
            // Instruction.{depends on context.Operator.Type}
            code.Add(Visit(context.expression()));
            code.Add(GetUnaryOperatorFromContext(context));

            return code;
        }

        public override AbstractBytecodeEmitter VisitBinaryExpression(HekateParser.BinaryExpressionContext context)
        {
            var code = new CodeScope();

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
    }
}
