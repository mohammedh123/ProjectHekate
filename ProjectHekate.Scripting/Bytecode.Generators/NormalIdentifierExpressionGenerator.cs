using ProjectHekate.Scripting.Helpers;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class NormalIdentifierExpressionGenerator : AbstractBytecodeEmitter
    {
        private readonly string _identifierName;

        public NormalIdentifierExpressionGenerator(string identifierName)
        {
            _identifierName = identifierName;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Normal identifier expression code:
            // Instructions.Push
            // {index of variable if it exists}

            return CodeGenHelper.GenerateCodeForValueOfVariable(scopeManager, _identifierName);
        }
    }
}
