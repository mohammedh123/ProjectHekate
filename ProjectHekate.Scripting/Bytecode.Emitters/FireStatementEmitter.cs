using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class FireStatementEmitter : EmptyEmitter
    {
        private readonly string _typeName;
        private readonly string _firingFunctionName;
        private readonly IBytecodeGenerator _parameterBytecodeGenerator;

        public FireStatementEmitter(string typeName, string firingFunctionName, IBytecodeGenerator parameterBytecodeGenerator)
        {
            _typeName = typeName;
            _firingFunctionName = firingFunctionName;
            _parameterBytecodeGenerator = parameterBytecodeGenerator;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Fire statement code
            // Generate code for each parameter
            // Fire
            // idx of thing

            var firingFunction = vm.GetFiringFunction(_typeName, _firingFunctionName);
            if (firingFunction == null) {
                throw new ArgumentException(String.Format("No firing function was found for this type name ({0})/firing function name ({1}).", _typeName, _firingFunctionName));
            }

            codeBlock.Add(_parameterBytecodeGenerator.Generate(vm, scopeManager));
            codeBlock.Add(Instruction.Fire);
            codeBlock.Add(firingFunction.Index);
        }
    }
}
