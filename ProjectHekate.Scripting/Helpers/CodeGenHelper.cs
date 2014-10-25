using System;
using System.Linq;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Helpers
{
    internal static class CodeGenHelper
    {
        internal static ICodeBlock GenerateCodeForGettingValueOfVariable(IScopeManager scopeManager, string name)
        {
            var scope = scopeManager.GetCurrentScope();
            var index = scope.GetSymbol(name).Index;

            var code = new CodeBlock();
            code.Add(Instruction.GetVariable);
            code.Add(index);

            return code;
        }

        internal static ICodeBlock GenerateCodeForGettingValueOfProperty(IVirtualMachine vm, string name)
        {
            var index = vm.GetPropertyIndex(name);

            var code = new CodeBlock();
            code.Add(Instruction.GetProperty);
            code.Add(index);

            return code;
        }

        internal static ICodeBlock GenerateCodeForSettingValueOfVariable(IScopeManager scopeManager, string name)
        {
            var scope = scopeManager.GetCurrentScope();
            var symbol = scope.GetSymbol(name);

            var code = new CodeBlock();

            if (symbol.Type != SymbolType.Numerical)
            {
                throw new InvalidOperationException("Cannot use assignment expression with non-numerical symbols.");
            }

            code.Add(Instruction.SetVariable);
            code.Add(symbol.Index);
            return code;
        }

        internal static ICodeBlock GenerateCodeForSettingValueOfProperty(IVirtualMachine vm, string name)
        {
            var index = vm.GetPropertyIndex(name);

            var code = new CodeBlock();
            code.Add(Instruction.SetProperty);
            code.Add(index);

            return code;
        }

        internal static void ThrowIfSymbolAlreadyExists(IGlobalSymbolContext globalSymbolContext, ISymbolContext localSymbolcontext,
            string variableName)
        {
            if (globalSymbolContext.HasGlobalSymbolDefined(variableName)) {
                throw new ArgumentException("A global symbol already exists with that name.", "variableName");
            }

            if (localSymbolcontext.HasSymbolDefined(variableName)) {
                throw new ArgumentException("A local symbol already exists with that name.", "variableName");
            }
        }
    }
}
