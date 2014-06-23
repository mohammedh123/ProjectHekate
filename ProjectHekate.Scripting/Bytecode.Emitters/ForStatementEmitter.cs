using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class ForStatementEmitter : EmptyEmitter
    {
        private readonly IBytecodeGenerator _forInit, _forConditional, _forUpdate;
        private readonly IBytecodeEmitter _bodyStatement;
        private readonly IList<int> _breakList;
        private readonly IList<int> _continueList;

        public ForStatementEmitter(IBytecodeGenerator forInit, IBytecodeGenerator forConditional, IBytecodeGenerator forUpdate, IBytecodeEmitter bodyStatement, IList<int> breakList, IList<int> continueList)
        {
            _forInit = forInit;
            _forConditional = forConditional;
            _forUpdate = forUpdate;
            _bodyStatement = bodyStatement;
            _breakList = breakList;
            _continueList = continueList;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // For statement code
            // Generate code for the initialization (if there is one)
            // Generate code for the test expression (if there is one)
            // Generate code for the increment expressions (if there are any)
            // Generate code for the body statement
            // Instruction.Jump
            // jump index
            
            if(_forInit != null) codeBlock.Add(_forInit.Generate(vm, scopeManager));

            var loopBeginIdx = codeBlock.Size;
            if(_forConditional != null) codeBlock.Add(_forConditional.Generate(vm, scopeManager));
            if(_forUpdate != null) codeBlock.Add(_forUpdate.Generate(vm, scopeManager));
            _bodyStatement.EmitTo(codeBlock, vm, scopeManager);
            
            codeBlock.Add(Instruction.Jump);
            codeBlock.Add(loopBeginIdx);

            // loop through all break locations and update them
            foreach (var idx in _breakList)
            {
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
