using System;
using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Helpers;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class CompoundAssignmentExpressionGenerator : EmptyEmitter
    {
        private readonly IBytecodeGenerator _valueExpression;
        private readonly IdentifierType _identifierType;
        private readonly string _identifierName;
        private readonly Instruction _op;

        /// <summary>
        /// Constructs a compound assignment expression generator.
        /// </summary>
        /// <param name="valueExpression">The bytecode generator that will generate the code for the value that will be compounded with the identifier's value</param>
        /// <param name="identifierType">The type of identifier</param>
        /// <param name="identifierName">The identifier name</param>
        /// <param name="op">The operator (+-*/) of this compound assignment</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the operator is not valid</exception>
        public CompoundAssignmentExpressionGenerator(IBytecodeGenerator valueExpression, IdentifierType identifierType, string identifierName, Instruction op)
        {
            switch (op) {
                case Instruction.OpMultiply:
                case Instruction.OpDivide:
                case Instruction.OpAdd:
                case Instruction.OpSubtract:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("op", op, "The operator must be *, /, +, or -.");
            }

            _valueExpression = valueExpression;
            _identifierType = identifierType;
            _identifierName = identifierName;
            _op = op;

            CodeGenHelper.CoaxIdentifierToProperName(_identifierType, ref _identifierName);
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // NOTE: this assignment only happens for numeral assignments
            // Compound assignment expression code:
            // {evaluate identifier's value, should place value on stack}
            // {evaluate valueExpression, should place value on stack}
            // {an Instruction depending on what kind of assignment}
            // Instruction.SetVariable or Instruction.SetProperty
            // {index of the variable}

            var code = new CodeBlock();

            IBytecodeGenerator valueGen;
            switch (_identifierType) {
                case IdentifierType.Property:
                    valueGen = new PropertyIdentifierExpressionGenerator(_identifierName);
                    break;
                case IdentifierType.Variable:
                    valueGen = new NormalIdentifierExpressionGenerator(_identifierName);
                    break;
                default:
                    throw new InvalidOperationException(
                        "A new type of identifier was added, but the CompoundAssignmentExpressionGenerator was not updated to reflect this.");
            }

            code.Add(valueGen.Generate(vm, scopeManager));
            code.Add(_valueExpression.Generate(vm, scopeManager));
            code.Add(_op);

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
                    var symbol = scope.GetSymbol(_identifierName);

                    if (symbol.Type != SymbolType.Numerical)
                    {
                        throw new InvalidOperationException("Cannot use assignment expression with non-numerical symbols.");
                    }

                    code.Add(Instruction.SetVariable);
                    code.Add(symbol.Index);

                    break;
                }
            }

            return code;
        }
    }
}
