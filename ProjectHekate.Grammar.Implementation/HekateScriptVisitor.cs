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
            var valueExpression = Visit(context.expression());
            var variableName = context.NormalIdentifier().GetText();

            return new VariableDeclarationStatementEmitter(valueExpression, variableName);
        }

        public override AbstractBytecodeEmitter VisitReturnStatement(HekateParser.ReturnStatementContext context)
        {
            var expressionGen = Visit(context.expression());
            
            return new ReturnStatementEmitter(expressionGen);
        }

        public override AbstractBytecodeEmitter VisitIfStatement(HekateParser.IfStatementContext context)
        {
            var ifBodyStatementGen = Visit(context.statement(0));
            var elseBodyStatement = context.statement(1);
            var elseBodyStatementGen = elseBodyStatement == null ? null : Visit(elseBodyStatement);
            var ifConditionGen = Visit(context.parExpression());

            return new IfStatementEmitter(ifBodyStatementGen, elseBodyStatementGen, ifConditionGen);
        }

        public override AbstractBytecodeEmitter VisitForStatement(HekateParser.ForStatementContext context)
        {
            var forInitCtx = context.forInit();
            var forCondCtx = context.expression();
            var forUpdateCtx = context.forUpdate();
            
            var forInitGen = forInitCtx == null ? null : Visit(forInitCtx);
            var forCondGen = forCondCtx == null ? null : Visit(forCondCtx);
            var forUpdateGen = forUpdateCtx == null ? null : Visit(forUpdateCtx);
            var bodyStatementGen = Visit(context.statement());

            return new ForStatementEmitter(forInitGen, forCondGen, forUpdateGen, bodyStatementGen);
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

        public override AbstractBytecodeEmitter VisitVariableDeclarationStatement(HekateParser.VariableDeclarationStatementContext context)
        {
            var varDeclarationGen = Visit(context.variableDeclaration());

            return varDeclarationGen;
        }

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
            return context.expressionList() == null ? new EmptyEmitter() : Visit(context.expressionList());
        }

        public override AbstractBytecodeEmitter VisitExpressionList(HekateParser.ExpressionListContext context)
        {
            var expressionList = context.expression().Select(Visit).Cast<IBytecodeGenerator>().ToList();

            return new ExpressionListGenerator(expressionList);
        }

        public override AbstractBytecodeEmitter VisitBlockStatement(HekateParser.BlockStatementContext context)
        {
            return Visit(context.block());
        }

        public override AbstractBytecodeEmitter VisitBlock(HekateParser.BlockContext context)
        {
            var statementEmitters = context.statement().Select(Visit).Cast<IBytecodeEmitter>().ToList();

            return new BlockEmitter(statementEmitters);
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
            var isNormalIdentifier = context.NormalIdentifier() != null;
            var isPropertyIdentifier = context.PropertyIdentifier() != null;

            IdentifierType identifierType;
            string identifierName;
            if (isNormalIdentifier)
            {
                identifierType = IdentifierType.Variable;
                identifierName = context.NormalIdentifier().GetText();
            }
            else if (isPropertyIdentifier)
            {
                identifierType = IdentifierType.Property;
                identifierName = context.PropertyIdentifier().GetText();
            }
            else {
                throw new InvalidOperationException(
                    "You forgot to add a case for another identifier type! Check the code for VisitPostIncDecExpression.");
            }

            var op = GetIncOrDecOperatorFromContext(context);
            return new PostIncDecExpressionGenerator(identifierType, identifierName, op);
        }

        public override AbstractBytecodeEmitter VisitAssignmentExpression(HekateParser.AssignmentExpressionContext context)
        {
            var isNormalIdentifier = context.NormalIdentifier() != null;
            var isPropertyIdentifier = context.PropertyIdentifier() != null;

            // determine whether its a variable or a property
            IdentifierType identifierType;
            string identifierName;
            if (isNormalIdentifier)
            {
                identifierType = IdentifierType.Variable;
                identifierName = context.NormalIdentifier().GetText();
            }
            else if (isPropertyIdentifier)
            {
                identifierType = IdentifierType.Property;
                identifierName = context.PropertyIdentifier().GetText();
            }
            else
            {
                throw new InvalidOperationException("You forgot to add a case for another identifier type! Check the code for VisitAssignmentExpression.");
            }

            var exprGen = Visit(context.expression());
            if (context.Operator.Type == HekateParser.ASSIGN) {
                return new SimpleAssignmentExpressionGenerator(exprGen, identifierType, identifierName);
            }
            else {
                var op = GetCompoundAssignmentOperatorFromContext(context);
                return new CompoundAssignmentExpressionGenerator(exprGen, identifierType, identifierName, op);
            }
        }

        public override AbstractBytecodeEmitter VisitFunctionCallExpression(HekateParser.FunctionCallExpressionContext context)
        {
            var functionName = context.NormalIdentifier().GetText();
            
            return new FunctionCallExpressionGenerator(Visit(context.parExpressionList()), functionName);
        }

        public override AbstractBytecodeEmitter VisitUnaryExpression(HekateParser.UnaryExpressionContext context)
        {
            var op = GetUnaryOperatorFromContext(context);

            return new UnaryExpressionGenerator(Visit(context.expression()), op);
        }

        public override AbstractBytecodeEmitter VisitBinaryExpression(HekateParser.BinaryExpressionContext context)
        {
            var leftExprGen = Visit(context.expression(0));
            var rightExprGen = Visit(context.expression(1));
            var op = GetBinaryOperatorFromContext(context);

            return new BinaryExpressionGenerator(leftExprGen, rightExprGen, op);
        }

        private Instruction GetIncOrDecOperatorFromContext(HekateParser.PostIncDecExpressionContext context)
        {
            switch (context.Operator.Type)
            {
                case HekateParser.INC: return Instruction.OperatorAdd;
                case HekateParser.DEC: return Instruction.OperatorSubtract;
                default: throw new InvalidOperationException("Invalid operator type found for this PostIncDecExpressionContext (" + context.Operator.Text + ").");
            }
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
            switch (context.Operator.Type) {
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
            switch (context.Operator.Type) {
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
