using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class BlockEmitter : EmptyEmitter
    {
        private readonly IList<IBytecodeEmitter> _statementEmitters;

        public BlockEmitter(IList<IBytecodeEmitter> statementEmitters)
        {
            _statementEmitters = statementEmitters;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            foreach (var emitter in _statementEmitters) {
                emitter.EmitTo(codeBlock, vm, scopeManager);
            }
        }
    }
}
