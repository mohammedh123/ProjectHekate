using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class ReturnStatementEmitter : EmptyEmitter
    {
        private readonly IBytecodeGenerator _expression;

        public ReturnStatementEmitter(IBytecodeGenerator expression)
        {
            _expression = expression;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Return statement code:
            // Generate code for expression
            // Instruction.Return

            codeBlock.Add(_expression.Generate(vm, scopeManager));
            codeBlock.Add(Instruction.Return);
        }
    }
}
