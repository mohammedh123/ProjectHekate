using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class WaitStatementEmitter : EmptyEmitter
    {
        private readonly IBytecodeGenerator _expression;

        public WaitStatementEmitter(IBytecodeGenerator expression)
        {
            _expression = expression;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Wait statement code:
            // Generate code for expression
            // Instruction.WaitFrames

            codeBlock.Add(_expression.Generate(vm, scopeManager));
            codeBlock.Add(Instruction.WaitFrames);
        }
    }
}
