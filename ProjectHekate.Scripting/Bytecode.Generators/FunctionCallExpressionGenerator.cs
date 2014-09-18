using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class FunctionCallExpressionGenerator : EmptyEmitter
    {
        private readonly IBytecodeGenerator _parametersExpression;
        private readonly string _functionName;

        public FunctionCallExpressionGenerator(IBytecodeGenerator parametersExpression, string functionName)
        {
            _parametersExpression = parametersExpression;
            _functionName = functionName;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Function call expression code:
            // Check if the function matches an external function registered to the vm
            //  if it has, then Instruction.
            // Generate code for each parameter value (each should push a value onto the stack)
            // Instruction.FunctionCall
            // {function code scope's index}

            var code = new CodeBlock();

            code.Add(_parametersExpression.Generate(vm, scopeManager));
            var extFunc = vm.GetExternalFunction(_functionName);
            if (extFunc != null) {
                code.Add(Instruction.ExternalFunctionCall);
                var externalFunctionIndex = extFunc.Index;
                code.Add(externalFunctionIndex);
            }
            else {
                code.Add(Instruction.FunctionCall);
                var functionIndex = vm.GetFunctionCodeScope(_functionName).Index;
                code.Add(functionIndex);
            }


            return code;
        }
    }
}
