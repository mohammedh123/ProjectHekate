using System.Collections.Generic;

namespace ProjectHekate.Scripting
{
    public class FunctionCodeScope : ParameterizedCodeScope
    {
        public FunctionCodeScope(IEnumerable<string> paramNames) : base(paramNames)
        {}
    }
}