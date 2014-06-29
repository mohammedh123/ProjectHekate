using System;
using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class BinaryExpressionGenerator : EmptyEmitter
    {
        private readonly IBytecodeGenerator _leftExpression, _rightExpression;
        private readonly Instruction _op;

        public BinaryExpressionGenerator(IBytecodeGenerator leftExpression, IBytecodeGenerator rightExpression, Instruction op)
        {
            // do some basic validation of the operator
            switch (op)
            {
                case Instruction.OpMultiply:
                case Instruction.OpDivide:
                case Instruction.OpMod:
                case Instruction.OpAdd:
                case Instruction.OpSubtract:
                case Instruction.OpLessThan:
                case Instruction.OpGreaterThan:
                case Instruction.OpLessThanEqual:
                case Instruction.OpGreaterThanEqual:
                case Instruction.OpEqual:
                case Instruction.OpNotEqual:
                case Instruction.OpAnd:
                case Instruction.OpOr:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("op", op, "The operator supplied is not a binary operator.");
            }

            _leftExpression = leftExpression;
            _rightExpression = rightExpression;
            _op = op;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Binary expression code:
            // Generate code for left expression (should push onto stack)
            // Generate code for right expression (should push onto stack)
            // Instruction.{depends on context.Operator.Type}

            var code = new CodeBlock();

            code.Add(_leftExpression.Generate(vm, scopeManager));
            code.Add(_rightExpression.Generate(vm, scopeManager));
            code.Add(_op);

            return code;
        }
    }
}
