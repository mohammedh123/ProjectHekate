namespace ProjectHekate.Scripting.Interfaces
{
    /// <summary>
    /// An interface for a class that generates bytecode.
    /// </summary>
    interface IBytecodeGenerator
    {
        ICodeBlock Generate(IPropertyContext propCtx, IScopeManager scopeManager);
    }
}