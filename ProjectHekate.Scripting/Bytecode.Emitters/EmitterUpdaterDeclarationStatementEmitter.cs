using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class EmitterUpdaterDeclarationStatementEmitter : EmptyEmitter
    {
        private readonly IReadOnlyList<string> _parameterNames;
        private readonly string _name;
        private readonly IReadOnlyList<IBytecodeEmitter> _statements;

        public EmitterUpdaterDeclarationStatementEmitter(IReadOnlyList<string> parameterNames, string name, IReadOnlyList<IBytecodeEmitter> statements)
        {
            _parameterNames = parameterNames;
            _name = name;
            _statements = statements;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            var eUpdaterCodeBlock = new EmitterUpdaterCodeScope(_parameterNames);

            scopeManager.Add(eUpdaterCodeBlock);
            foreach (var statement in _statements)
            {
                statement.EmitTo(eUpdaterCodeBlock, vm, scopeManager);
            }
            scopeManager.Remove();

            // done, now add to the pool of emitter updater records
            vm.AddEmitterUpdaterCodeScope(_name, eUpdaterCodeBlock);
        }
    }
}
