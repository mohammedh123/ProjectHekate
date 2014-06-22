namespace ProjectHekate.Scripting.Interfaces
{
    /// <summary>
    /// An interface for a class that generates bytecode.
    /// </summary>
    public interface IBytecodeGenerator
    {
        ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager);
    }
}