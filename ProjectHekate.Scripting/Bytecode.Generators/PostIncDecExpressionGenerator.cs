using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class PostIncDecExpressionGenerator : AbstractBytecodeEmitter
    {
        private readonly IdentifierType _identifierType;
        private readonly string _identifierName;
        private readonly Instruction _op;

        public PostIncDecExpressionGenerator(IdentifierType identifierType, string identifierName, Instruction op)
        {
            // basic validation of operator
            switch (op)
            {
                case Instruction.OperatorAdd:
                case Instruction.OperatorSubtract:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("op", op, "The operator must be either the addition operator or the subtraction operator.");
            }

            _identifierType = identifierType;
            _identifierName = identifierName;
            _op = op;
        }
        
        public override ICodeBlock Generate(IPropertyContext propCtx, IScopeManager scopeManager)
        {           
            // Post-inc/decrement expression code:
            // Instruction.GetVariable/Property
            // {index of variable/property}
            // Instructions.Push
            // {1}
            // Instruction.OperatorAdd/Subtract
            // Instruction.SetVariable/Property
            // {index of variable/property}

            var code = new CodeBlock();

            var oneLitGen = new LiteralExpressionGenerator(1);
            var compAssignExprGen = new CompoundAssignmentExpressionGenerator(oneLitGen, _identifierType, _identifierName, _op);

            code.Add(compAssignExprGen.Generate(propCtx, scopeManager));

            return code;
        }
    }
}
