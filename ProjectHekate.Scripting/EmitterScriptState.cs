using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting
{
    class EmitterScriptState
    {
        public int CurrentWaitTime { get; set; }

        public Stack<float> Stack { get; set; }

        public EmitterScriptState()
        {
            Stack = new Stack<float>();
        }
    }
}
