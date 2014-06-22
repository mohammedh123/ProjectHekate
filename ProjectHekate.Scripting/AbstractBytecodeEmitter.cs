using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public abstract class AbstractBytecodeEmitter : IBytecodeEmitter
    {
        public virtual ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            return new CodeBlock();
        }

        public virtual void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {}
    }
}