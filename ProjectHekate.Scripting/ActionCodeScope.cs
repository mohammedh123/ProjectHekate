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
    }
}