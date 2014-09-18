using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class LiteralExpressionGenerator : EmptyEmitter
    {
        private readonly float _value;
        
        public LiteralExpressionGenerator(float value)
        {
            _value = value;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            var code = new CodeBlock();

            code.Add(Instruction.Push);
            code.Add(_value);

            return code;
        }
    }
}
