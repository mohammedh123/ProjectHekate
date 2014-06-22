using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class LiteralExpressionGenerator : AbstractBytecodeEmitter
    {
        private readonly float _value;
        
        public LiteralExpressionGenerator(float value)
        {
            _value = value;
        }

        public override ICodeBlock Generate(IPropertyContext propCtx, IScopeManager scopeManager)
        {
            var code = new CodeBlock();

            code.Add(Instruction.Push);
            code.Add(_value);

            return code;
        }
    }
}
