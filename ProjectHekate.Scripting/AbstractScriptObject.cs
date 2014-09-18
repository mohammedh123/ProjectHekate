using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting
{
    public class AbstractScriptObject
    {
        public ScriptState ScriptState { get; set; }

        public int EmitTypeIndex { get; set; }
    }
}
