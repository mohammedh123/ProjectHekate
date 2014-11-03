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
        private readonly int _numParams;
        private readonly IBytecodeGenerator _parameterBytecodeGenerator;
        private readonly IBytecodeGenerator _updaterCallParamGenerator;
        private readonly int _numParamsOnUpdater;
        private readonly string _updaterName;

        public FireStatementEmitter(string typeName, string firingFunctionName, int numParams, IBytecodeGenerator parameterBytecodeGenerator) : this(typeName, firingFunctionName, numParams, parameterBytecodeGenerator, null, 0, null)
        {}

        public FireStatementEmitter(string typeName, string firingFunctionName, int numParams, IBytecodeGenerator parameterBytecodeGenerator, IBytecodeGenerator updaterCallParamGenerator, int numParamsOnUpdater, string updaterName)
        {
            _typeName = typeName;
            _firingFunctionName = firingFunctionName;
            _numParams = numParams;
            _parameterBytecodeGenerator = parameterBytecodeGenerator;
            _updaterCallParamGenerator = updaterCallParamGenerator;
            _numParamsOnUpdater = numParamsOnUpdater;
            _updaterName = updaterName;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Fire statement code
            // Generate code for each parameter
            // Fire
            // idx of thing

            var firingFunction = vm.GetFiringFunction(_typeName, _firingFunctionName);
            if (firingFunction == null) {
                throw new ArgumentException(
                    String.Format("No firing function was found for this type name ({0})/firing function name ({1}).", _typeName,
                        _firingFunctionName));
            }

            if (firingFunction.NumParams != _numParams) {
                throw new ArgumentException(
                    String.Format("No firing function with the name \"{0}\" and for the type \"{1}\" with {2} arguments was found.",
                        _firingFunctionName, _typeName, _numParams));
            }

            var isWithUpdater = _updaterCallParamGenerator != null;
            ActionCodeScope updaterAction = null;
            if (isWithUpdater) {
                updaterAction = vm.GetActionCodeScope(_updaterName);
                if (updaterAction == null) {
                    throw new ArgumentException(String.Format("No updater was found for with the name \"{0}\".", _updaterName));
                }

                if (updaterAction.NumParams != _numParamsOnUpdater) {
                    throw new ArgumentException(String.Format("No updater with the name \"{0}\" with {1} arguments was found.", _updaterName,
                        _numParamsOnUpdater));
                }
            }

            codeBlock.Add(_parameterBytecodeGenerator.Generate(vm, scopeManager));

            // this is with updater, then
            if (isWithUpdater) {
                codeBlock.Add(_updaterCallParamGenerator.Generate(vm, scopeManager));
            }
            
            codeBlock.Add(isWithUpdater ? Instruction.FireWithUpdater : Instruction.Fire);
            codeBlock.Add(firingFunction.Index);

            if (isWithUpdater) {
                codeBlock.Add(updaterAction.Index);
            }
        }
    }
}
