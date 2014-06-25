using System.Collections.Generic;

namespace ProjectHekate.Scripting
{
    public class EmitterUpdaterCodeScope : CodeScope
    {
        public EmitterUpdaterCodeScope(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddSymbol(paramName, SymbolTypes.Numerical); // emitter updaters can only have numerical params
            }
        }
    }
}