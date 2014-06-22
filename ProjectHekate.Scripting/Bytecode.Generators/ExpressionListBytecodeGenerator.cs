using System.Collections.Generic;
using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class ExpressionListBytecodeGenerator : EmptyBytecodeEmitter
    {
        private readonly IList<IBytecodeGenerator> _expressionList;

        public ExpressionListBytecodeGenerator(IList<IBytecodeGenerator> expressionList)
        {
            _expressionList = expressionList;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            var code = new CodeBlock();

            foreach (var expression in _expressionList) {
                code.Add(expression.Generate(vm, scopeManager));
            }

            return code;
        }
    }
}
