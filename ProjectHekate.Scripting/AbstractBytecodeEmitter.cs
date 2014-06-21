using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public abstract class AbstractBytecodeEmitter : IBytecodeEmitter
    {
        public virtual ICodeBlock Generate(IPropertyContext propCtx, IScopeManager scopeManager)
        {
            return new CodeBlock();
        }

        public virtual void EmitTo(ICodeBlock codeBlock, IPropertyContext propCtx, IScopeManager scopeManager)
        {}
    }
}