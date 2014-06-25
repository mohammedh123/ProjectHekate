using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class VariableDeclarationStatementEmitter : EmptyEmitter
    {        
        private readonly IBytecodeGenerator _valueExpression;
        private readonly string _variableName;

        public VariableDeclarationStatementEmitter(IBytecodeGenerator valueExpression, string variableName)
        {
            _valueExpression = valueExpression;
            _variableName = variableName;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // NOTE: this declaration only happens for numeral assignments
            // Variable declaration code:
            // {evaluate expression, should place value on stack}
            // Instruction.SetVariable
            // {index of the variable}

            var code = new CodeBlock();

            var currentScope = scopeManager.GetCurrentScope();
            var index = currentScope.AddSymbol(_variableName, SymbolTypes.Numerical);

            code.Add(_valueExpression.Generate(vm, scopeManager));
            code.Add(Instruction.SetVariable);
            code.Add(index);

            return code;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            var code = Generate(vm, scopeManager);

            codeBlock.Add(code);
        }
    }
}
