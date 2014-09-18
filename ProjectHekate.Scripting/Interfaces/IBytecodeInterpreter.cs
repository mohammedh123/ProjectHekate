using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface IBytecodeInterpreter
    {
        ScriptStatus InterpretCode(ICodeBlock code, ScriptState state, AbstractScriptObject obj, bool looping);
    }
}
