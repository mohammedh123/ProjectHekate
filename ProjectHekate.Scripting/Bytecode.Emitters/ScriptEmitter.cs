using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class ScriptEmitter : EmptyEmitter
    {
        private readonly IReadOnlyList<IBytecodeEmitter> _codeScopeEmitters;

        public ScriptEmitter(IReadOnlyList<IBytecodeEmitter> codeScopeEmitters)
        {
            _codeScopeEmitters = codeScopeEmitters;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            foreach (var emitter in _codeScopeEmitters) {
                emitter.EmitTo(null, vm, scopeManager);
            }
        }
    }
}
