using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class ExpressionStatementEmitter : EmptyEmitter
    {
        private readonly IBytecodeGenerator _expressionCodeGenerator;

        public ExpressionStatementEmitter(IBytecodeGenerator expressionCodeGenerator)
        {
            _expressionCodeGenerator = expressionCodeGenerator;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            var expressionCode = _expressionCodeGenerator.Generate(vm, scopeManager);

            codeBlock.Add(expressionCode);
            codeBlock.Add(Instruction.Pop);
        }
    }
}
