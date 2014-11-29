using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Bytecode.Emitters;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Generators
{
    public class TernaryOpExpressionGenerator : EmptyEmitter
    {
        private readonly IBytecodeGenerator _testExpression, _trueExpression, _falseExpression;

        public TernaryOpExpressionGenerator(IBytecodeGenerator testExpression, IBytecodeGenerator trueExpression, IBytecodeGenerator falseExpression)
        {
            _testExpression = testExpression;
            _trueExpression = trueExpression;
            _falseExpression = falseExpression;
        }

        public override ICodeBlock Generate(IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Ternary Op code
            // Generate test expression
            // Instruction.IfZeroBranchOffset
            // The offset to branch to if testExpression is 0 (which should be the location of the falseExpression code
            // trueExpression
            // Instruction.Jump to end of block
            // falseExpression

            var code = new CodeBlock();

            code.Add(_testExpression.Generate(vm, scopeManager));

            code.Add(Instruction.IfZeroBranchOffset);
            var branchIdx = code.Size;
            code.Add(0.0f);

            var trueExpressionCode = _trueExpression.Generate(vm, scopeManager);
            code.Add(trueExpressionCode);

            code.Add(Instruction.JumpOffset);
            var ifTrueJumpIdx = code.Size;
            code.Add(0.0f);

            var falseExpressionCode = _falseExpression.Generate(vm, scopeManager);
            code.Add(falseExpressionCode);

            code[branchIdx] = trueExpressionCode.Size + 2;
            code[ifTrueJumpIdx] = falseExpressionCode.Size;

            return code;
        }
    }
}
