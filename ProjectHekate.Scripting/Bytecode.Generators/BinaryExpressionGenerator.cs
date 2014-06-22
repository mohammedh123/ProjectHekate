using System;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class BinaryExpressionGenerator : AbstractBytecodeEmitter
    {
        private readonly IBytecodeGenerator _leftExpression, _rightExpression;
        private readonly Instruction _op;

        public BinaryExpressionGenerator(IBytecodeGenerator leftExpression, IBytecodeGenerator rightExpression, Instruction op)
        {
            // do some basic validation of the operator
            switch (op)
            {
                case Instruction.OperatorMultiply:
                case Instruction.OperatorDivide:
                case Instruction.OperatorMod:
                case Instruction.OperatorAdd:
                case Instruction.OperatorSubtract:
                case Instruction.OperatorLessThan:
                case Instruction.OperatorGreaterThan:
                case Instruction.OperatorLessThanEqual:
                case Instruction.OperatorGreaterThanEqual:
                case Instruction.OperatorEqual:
                case Instruction.OperatorNotEqual:
                case Instruction.OperatorAnd:
                case Instruction.OperatorOr:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("op", op, "The operator supplied is not a binary operator.");
            }

            _leftExpression = leftExpression;
            _rightExpression = rightExpression;
            _op = op;
        }

        public override ICodeBlock Generate(IPropertyContext propCtx, IScopeManager scopeManager)
        {
            // Binary expression code:
            // Generate code for left expression (should push onto stack)
            // Generate code for right expression (should push onto stack)
            // Instruction.{depends on context.Operator.Type}

            var code = new CodeBlock();

            code.Add(_leftExpression.Generate(propCtx, scopeManager));
            code.Add(_rightExpression.Generate(propCtx, scopeManager));
            code.Add(_op);

            return code;
        }
    }
}
