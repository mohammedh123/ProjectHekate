using System;
using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class UnaryExpressionGenerator : EmptyEmitter
    {
        private readonly IBytecodeGenerator _expression;
        private readonly Instruction _op;

        public UnaryExpressionGenerator(IBytecodeGenerator expression, Instruction op)
        {
            switch (op) {
                case Instruction.Negate:
                case Instruction.OpNot:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("op", op, "Invalid unary operator supplied.");
            }

            _expression = expression;
            _op = op;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Unary expression code:
            // Generate code for expression (should push onto stack)
            // op

            var code = new CodeBlock();

            code.Add(_expression.Generate(vm, scopeManager));
            code.Add(_op);

            return code;
        }
    }
}
