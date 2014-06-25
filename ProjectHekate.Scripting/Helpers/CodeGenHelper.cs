using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Helpers
{
    internal static class CodeGenHelper
    {
        internal static ICodeBlock GenerateCodeForValueOfVariable(IScopeManager scopeManager, string name)
        {
            var scope = scopeManager.GetCurrentScope();
            var index = scope.GetSymbol(name).Index;

            var code = new CodeBlock();
            code.Add(Instruction.GetVariable);
            code.Add(index);

            return code;
        }

        internal static ICodeBlock GenerateCodeForValueOfProperty(IPropertyContext propCtx, string name)
        {
            var index = propCtx.GetProperty(name).Index;

            var code = new CodeBlock();
            code.Add(Instruction.GetProperty);
            code.Add(index);

            return code;
        }
    }
}
