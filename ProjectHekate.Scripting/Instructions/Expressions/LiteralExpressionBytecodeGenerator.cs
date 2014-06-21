using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Instructions.Expressions
{
    class LiteralExpressionBytecodeGenerator : IBytecodeGenerator
    {
        private readonly float _value;

        public LiteralExpressionBytecodeGenerator(float value)
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
