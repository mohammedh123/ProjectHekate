namespace ProjectHekate.Scripting.Interfaces
{
    /// <summary>
    /// An interface for a class generates bytecode.
    /// </summary>
    interface IBytecodeGenerator
    {
        /// <summary>
        /// Emits code for this instruction on this virtual machine.
        /// </summary>
        /// <param name="vm">The virtual machine to execute the instruction on.</param>
        /// <param name="scopeManager">The scope manager</param>
        void EmitOn(IVirtualMachine vm, IScopeManager scopeManager);
    }
}
 