namespace ProjectHekate.Scripting.Interfaces
{
    /// <summary>
    /// An interface for a class that both generates and emits bytecode.
    /// </summary>
    interface IBytecodeEmitter : IBytecodeGenerator
    {
        void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager);
    }
}