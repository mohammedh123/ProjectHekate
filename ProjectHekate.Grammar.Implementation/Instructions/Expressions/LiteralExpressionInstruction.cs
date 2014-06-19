using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Grammar.Implementation.Interfaces;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation.Instructions.Expressions
{
    class LiteralExpressionInstruction : IInstruction
    {
        private readonly float _value;

        public LiteralExpressionInstruction(float value)
        {
            _value = value;
        }

        public void EmitOn(IVirtualMachine vm, IScopeManager scopeManager)
        {
            vm.CurrentCode.Add(Instruction.Push);
            vm.CurrentCode.Add(_value);
        }
    }
}
