namespace ProjectHekate.Scripting.Interfaces
{
    /// <summary>
    /// An interface for a class that both generates and emits bytecode.
    /// </summary>
    public interface IBytecodeEmitter : IBytecodeGenerator
    {
        void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager);
    }
}