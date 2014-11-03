using System.Collections.Generic;
using System.Linq;

namespace ProjectHekate.Scripting
{
    public class ActionCodeScope : ParameterizedCodeScope
    {
        public ActionCodeScope(IEnumerable<string> paramNames) : base(paramNames)
        {}

        public ActionCodeScope(ActionCodeScope existingCodeScope, CodeBlock newCode) : base(existingCodeScope.GetSymbols().Select(sym => sym.Name))
        {
            Index = existingCodeScope.Index;

            Add(newCode);
        }
    }
}