using ProjectHekate.Scripting.Helpers;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class PropertyIdentifierExpressionGenerator : AbstractBytecodeEmitter
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

            return CodeGenHelper.GenerateCodeForValueOfProperty(vm, _propertyIdentifier);
        }
    }
}
