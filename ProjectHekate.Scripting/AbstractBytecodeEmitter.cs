using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public abstract class AbstractBytecodeEmitter : IBytecodeEmitter
    {
        public abstract ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager);
        public abstract void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager);
    }
}