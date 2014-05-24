using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation
{
    class BytecodeCompiler
    {
        private readonly IVirtualMachine _virtualMachine;

        public BytecodeCompiler(IVirtualMachine virtualMachine)
        {
            _virtualMachine = virtualMachine;
        }

        public CodeBlock GenerateConstantExpression(VirtualMachine vm)
        {
            return null;
        }
    }
}
