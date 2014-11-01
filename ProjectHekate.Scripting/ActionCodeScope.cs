using System.Collections.Generic;

namespace ProjectHekate.Scripting
{
    public class ActionCodeScope : CodeScope
    {
        public ActionCodeScope(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddSymbol(paramName, SymbolType.Numerical); // bullet updaters can only have numerical params
            }
        }

        public ActionCodeScope(ActionCodeScope existingCodeScope, CodeBlock newCode)
        {
            foreach (var symbol in existingCodeScope.GetSymbols()) {
                AddSymbol(symbol.Name, symbol.Type);
            }

            Index = existingCodeScope.Index;

            Add(newCode);
        }
    }
}