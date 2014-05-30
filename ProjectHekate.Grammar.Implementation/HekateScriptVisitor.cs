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

            var paramNames = paramContexts.Select(fpc => fpc.Identifier().GetText());
            var name = context.Identifier().GetText();


            var bUpdaterCodeBlock = new BulletUpdaterCodeBlock(paramNames);
            _scopeManager.Add(bUpdaterCodeBlock);
            foreach (var child in context.children) {
                bUpdaterCodeBlock.Add(Visit(child));
            }
            _scopeManager.Remove();

            // done, now add to the pool of bullet updater records
            _virtualMachine.AddBulletUpdaterCodeBlock(name, bUpdaterCodeBlock);
            
            return bUpdaterCodeBlock;
        }

        public override CodeBlock VisitBulletUpdaterDeclaration(HekateParser.BulletUpdaterDeclarationContext context)
        {
            return base.VisitBulletUpdaterDeclaration(context);
        }

        public override CodeBlock VisitFunctionDeclaration(HekateParser.FunctionDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();

            var paramNames = paramContexts.Select(fpc => fpc.Identifier().GetText());
            var name = context.Identifier().GetText();


            var funcCodeBlock = new FunctionCodeBlock(paramNames);
            _scopeManager.Add(funcCodeBlock);
            foreach (var child in context.children)
            {
                funcCodeBlock.Add(Visit(child));
            }
            _scopeManager.Remove();

            // done, now add to the pool of bullet updater records
            _virtualMachine.AddFunctionCodeBlock(name, funcCodeBlock);

            return funcCodeBlock;
        }


        #endregion

        public override CodeBlock VisitExpressionStatement(HekateParser.ExpressionStatementContext context)
        {
            return base.VisitExpressionStatement(context);
        }

        public override CodeBlock VisitParenthesizedExpression(HekateParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expression());
        }

        #region Statements
        
        public override CodeBlock VisitVariableDeclaration(HekateParser.VariableDeclarationContext context)
        {
            var code = new CodeBlock();
            var scope = _scopeManager.GetCurrentScope();

            var variableName = context.Identifier().GetText();
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

        #endregion
        

        #region Expression constructs

        public override CodeBlock VisitAssignmentExpression(HekateParser.AssignmentExpressionContext context)
        {
            var code = new CodeBlock();
            var scope = _scopeManager.GetCurrentScope();

            var variableName = context.Identifier().GetText();
            var index = scope.GetNumericalVariable(variableName).Index;

            // Variable declaration code:
            // (exact same as variable declaration code), only difference will be that the parser checks
            // to make sure the variable exists; otherwise, it throws an exception

            code.Add(Visit(context.expression()));
            code.Add(Instruction.SetVariable);
            code.Add(index);

            return code;
        }

        public override CodeBlock VisitAddAssignmentExpression(HekateParser.AddAssignmentExpressionContext context)
        {
            var code = new CodeBlock();
            var identifierName = context.Identifier().GetText();

            var currentScope = _scopeManager.GetCurrentScope();
            
            // Add assignment code:
            // Instruction.Assign
            // [Identifier of some sort: property, or local variable]
            // Uses Stack[0] (top of stack) for value
            
            return null;
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

        public override CodeBlock VisitLiteralExpression(HekateParser.LiteralExpressionContext context)
        {
            var code = new CodeBlock();
            var text = context.GetText();
            var value = float.Parse(text);

            code.Add(Instruction.Push);
            code.Add(value);

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


        #endregion
    }
}
