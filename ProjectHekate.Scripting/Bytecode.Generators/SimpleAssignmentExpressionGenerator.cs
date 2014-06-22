using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class SimpleAssignmentExpressionGenerator : EmptyBytecodeEmitter
    {
        private readonly IBytecodeGenerator _valueToAssignExpression;
        private readonly IdentifierType _identifierType;
        private readonly string _identifierName;

        public SimpleAssignmentExpressionGenerator(IBytecodeGenerator valueToAssignExpression, IdentifierType identifierType, string identifierName)
        {
            _valueToAssignExpression = valueToAssignExpression;
            _identifierType = identifierType;
            _identifierName = identifierName;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // NOTE: this assignment only happens for numeral assignments
            // Simple assignment expression code:
            // {evaluate valueToAssignExpression, should place value on stack}
            // Instruction.SetVariable or Instruction.SetProperty
            // {index of the variable}

            var code = new CodeBlock();

            code.Add(_valueToAssignExpression.Generate(vm, scopeManager));

            switch (_identifierType) {
                case IdentifierType.Property:
                {
                    var index = vm.GetProperty(_identifierName).Index;

                    code.Add(Instruction.SetProperty);
                    code.Add(index);

                    break;
                }
                case IdentifierType.Variable:
                {
                    var scope = scopeManager.GetCurrentScope();
                    var index = scope.GetNumericalVariable(_identifierName).Index;

                    code.Add(Instruction.SetVariable);
                    code.Add(index);

                    break;
                }
                default:    
                    throw new InvalidOperationException("A new type of identifier was added, but the SimpleAssignmentExpressionGenerator was not updated to reflect this.");
            }

            return code;
        }
    }
}
