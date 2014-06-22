using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Helpers;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class CompoundAssignmentExpressionGenerator : AbstractBytecodeEmitter
    {
        private readonly IBytecodeGenerator _valueToAssignExpression;
        private readonly IdentifierType _identifierType;
        private readonly string _identifierName;
        private readonly Instruction _op;

        /// <summary>
        /// Constructs a compound assignment expression generator.
        /// </summary>
        /// <param name="valueToAssignExpression">The bytecode generator that will generate the code for the assignment's value</param>
        /// <param name="identifierType">The type of identifier</param>
        /// <param name="identifierName">The identifier name</param>
        /// <param name="op">The operator (+-*/) of this compound assignment</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the operator is not valid</exception>
        public CompoundAssignmentExpressionGenerator(IBytecodeGenerator valueToAssignExpression, IdentifierType identifierType, string identifierName, Instruction op)
        {
            switch (op) {
                case Instruction.OperatorMultiply:
                case Instruction.OperatorDivide:
                case Instruction.OperatorAdd:
                case Instruction.OperatorSubtract:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("op", op, "The operator must be *, /, +, or -.");
            }

            _valueToAssignExpression = valueToAssignExpression;
            _identifierType = identifierType;
            _identifierName = identifierName;
            _op = op;
        }

        public override ICodeBlock Generate(IPropertyContext propCtx, IScopeManager scopeManager)
        {
            // NOTE: this assignment only happens for numeral assignments
            // Compound assignment expression code:
            // {evaluate identifier's value, should place value on stack}
            // {evaluate valueToAssignExpression, should place value on stack}
            // {an Instruction depending on what kind of assignment}
            // Instruction.SetVariable or Instruction.SetProperty
            // {index of the variable}

            var code = new CodeBlock();

            var propValueGen = new PropertyIdentifierExpressionGenerator(_identifierName);
            code.Add(propValueGen.Generate(propCtx, scopeManager));
            code.Add(_valueToAssignExpression.Generate(propCtx, scopeManager));
            code.Add(_op);

            switch (_identifierType) {
                case IdentifierType.Property:
                {
                    var index = propCtx.GetProperty(_identifierName).Index;
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
                    throw new InvalidOperationException(
                        "A new type of identifier was added, but the SimpleAssignmentExpressionGenerator was not updated to reflect this.");
            }

            return code;
        }
    }
}
