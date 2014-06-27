using System.Collections.Generic;

namespace ProjectHekate.Scripting
{
    public class BulletUpdaterCodeScope : CodeScope
    {
        public BulletUpdaterCodeScope(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddSymbol(paramName, SymbolType.Numerical); // bullet updaters can only have numerical params
            }
        }
    }
}