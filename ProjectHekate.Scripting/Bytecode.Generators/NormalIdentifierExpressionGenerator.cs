using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Helpers;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class NormalIdentifierExpressionGenerator : EmptyEmitter
    {
        private readonly string _identifierName;

        public NormalIdentifierExpressionGenerator(string identifierName)
        {
            _identifierName = identifierName;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // if the identifier belongs to a global value, then just push that value
            // else
            // Normal identifier expression code:
            // Instructions.GetVariable
            // {index of variable if it exists}

            if (vm.HasGlobalSymbolDefined(_identifierName)) {
                var codeBlock = new CodeBlock();
                codeBlock.Add(Instruction.Push);
                codeBlock.Add(vm.GetGlobalSymbolValue(_identifierName));

                return codeBlock;
            }

            return CodeGenHelper.GenerateCodeForGettingValueOfVariable(scopeManager, _identifierName);
        }
    }
}
