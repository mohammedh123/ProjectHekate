using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class UnaryExpressionGenerator : AbstractBytecodeEmitter
    {
        private readonly IBytecodeGenerator _expression;
        private readonly Instruction _op;

        public UnaryExpressionGenerator(IBytecodeGenerator expression, Instruction op)
        {
            switch (op) {
                case Instruction.Negate:
                case Instruction.OperatorNot:
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

            var code = new CodeScope();

            code.Add(_expression.Generate(vm, scopeManager));
            code.Add(_op);

            return code;
        }
    }
}
