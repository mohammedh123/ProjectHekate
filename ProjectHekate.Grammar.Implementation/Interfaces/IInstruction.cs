using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation.Interfaces
{
    /// <summary>
    /// An interface for an instruction that generates bytecode.
    /// </summary>
    interface IInstruction
    {
        /// <summary>
        /// Emits code for this instruction on this virtual machine.
        /// </summary>
        /// <param name="vm">The virtual machine to execute the instruction on.</param>
        /// <param name="scopeManager">The scope manager</param>
        void EmitOn(IVirtualMachine vm, IScopeManager scopeManager);
    }
}
 