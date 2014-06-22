using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class FunctionCallExpressionGenerator : AbstractBytecodeEmitter
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
            // Generate code for each parameter value (each should push a value onto the stack)
            // Instruction.FunctionCall
            // {function code scope's index}

            var code = new CodeBlock();

            code.Add(_parametersExpression.Generate(vm, scopeManager));
            code.Add(Instruction.FunctionCall);
            
            var functionIndex = vm.GetFunctionCodeScope(_functionName).Index;
            code.Add(functionIndex);

            return code;
        }
    }
}
