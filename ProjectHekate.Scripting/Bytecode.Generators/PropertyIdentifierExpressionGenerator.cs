using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Helpers;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class PropertyIdentifierExpressionGenerator : EmptyEmitter
    {
        private readonly string _propertyIdentifier;

        public PropertyIdentifierExpressionGenerator(string propertyIdentifier)
        {
            _propertyIdentifier = propertyIdentifier;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Property identifier expression code:
            // Instructions.Push
            // {index of property if it exists}

            return CodeGenHelper.GenerateCodeForGettingValueOfProperty(vm, _propertyIdentifier);
        }
    }
}
