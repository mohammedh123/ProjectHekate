using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class FunctionDeclarationStatementEmitter : EmptyEmitter
    {
        private readonly IReadOnlyList<string> _parameterNames;
        private readonly string _name;
        private readonly IReadOnlyList<IBytecodeEmitter> _statements;

        public FunctionDeclarationStatementEmitter(IReadOnlyList<string> parameterNames, string name, IReadOnlyList<IBytecodeEmitter> statements)
        {
            _parameterNames = parameterNames;
            _name = name;
            _statements = statements;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            var funcCodeBlock = new FunctionCodeScope(_parameterNames);

            scopeManager.Add(funcCodeBlock);
            foreach (var statement in _statements)
            {
                statement.EmitTo(funcCodeBlock, vm, scopeManager);
            }
            scopeManager.Remove();

            // done, now add to the pool of function records
            vm.AddFunctionCodeScope(_name, funcCodeBlock);
        }
    }
}
