using System;
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

        public static void CoaxIdentifierToProperName(IdentifierType identifierType, ref string identifierName)
        {
            switch (identifierType) {
                case IdentifierType.Property:
                    identifierName = identifierName.Substring(1); // get rid of the first character
                    break;
                case IdentifierType.Variable:
                    // dont need to coax variable identifierName to anything
                    break;
                default:
                    throw new ArgumentOutOfRangeException("identifierType");
            }
        }
    }
}
