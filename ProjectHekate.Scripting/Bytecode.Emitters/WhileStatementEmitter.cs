using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class WhileStatementEmitter : EmptyEmitter
    {
        private readonly IBytecodeGenerator _conditionExpression;
        private readonly IBytecodeEmitter _bodyStatement;
        private readonly IList<int> _breakList;
        private readonly IList<int> _continueList;

        public WhileStatementEmitter(IBytecodeGenerator conditionExpression, IBytecodeEmitter bodyStatement, IList<int> breakList, IList<int> continueList)
        {
            _conditionExpression = conditionExpression;
            _bodyStatement = bodyStatement;
            _breakList = breakList;
            _continueList = continueList;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // While statement code
            // Generate code for _conditionExpression
            // Instruction.Jump + Pop
            // jump location
            // Generate expression code for bodyStatement
            // Instruction.Jump
            // jump location (should be index of conditional)
            // Pop (for early jumps)

            var loopBeginIdx = codeBlock.Size;
            codeBlock.Add(_conditionExpression.Generate(vm, scopeManager));
            codeBlock.Add(Instruction.IfZeroBranch);

            var whileJumpIdx = codeBlock.Size;
            codeBlock.Add(0.0f); // dummy value
            codeBlock.Add(Instruction.Pop);

            _bodyStatement.EmitTo(codeBlock, vm, scopeManager);

            codeBlock.Add(Instruction.Jump);
            codeBlock.Add(loopBeginIdx);
            
            codeBlock[whileJumpIdx] = codeBlock.Size; // this must be before the Pop so that the while jump pops after exiting the loop

            codeBlock.Add(Instruction.Pop);

            // loop through all break locations and update them
            foreach (var idx in _breakList) {
                codeBlock[idx] = codeBlock.Size;
            }
            _breakList.Clear();

            // loop through all continue locations and update them
            foreach (var idx in _continueList)
            {
                codeBlock[idx] = loopBeginIdx;
            }
            _continueList.Clear();
        }
    }
}
