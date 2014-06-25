using System.Collections.Generic;

namespace ProjectHekate.Scripting
{
    public class FunctionCodeScope : CodeScope
    {
        public FunctionCodeScope(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddSymbol(paramName, SymbolTypes.Numerical); // functions can only have numerical params
            }
        }
    }
}