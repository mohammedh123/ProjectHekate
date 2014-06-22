using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class ExpressionStatementEmitter : AbstractBytecodeEmitter
    {
        private readonly IBytecodeGenerator _expressionCodeGenerator;

        public ExpressionStatementEmitter(IBytecodeGenerator expressionCodeGenerator)
        {
            _expressionCodeGenerator = expressionCodeGenerator;
        }

        public override void EmitTo(ICodeBlock codeBlock, IPropertyContext propCtx, IScopeManager scopeManager)
        {
            var expressionCode = _expressionCodeGenerator.Generate(propCtx, scopeManager);

            codeBlock.Add(expressionCode);
            codeBlock.Add(Instruction.Pop);
        }
    }
}
