using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class EmptyEmitter : AbstractBytecodeEmitter
    {
        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            return new CodeBlock();
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {}
    }
}