﻿using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class WhileStatementEmitter : EmptyEmitter
    {
        private readonly IBytecodeGenerator _conditionExpression;
        private readonly IBytecodeEmitter _bodyStatement;

        public WhileStatementEmitter(IBytecodeGenerator conditionExpression, IBytecodeEmitter bodyStatement)
        {
            _conditionExpression = conditionExpression;
            _bodyStatement = bodyStatement;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // While statement code
            // Generate code for _conditionExpression
            // Instruction.Jump
            // jump location
            // Generate expression code for bodyStatement
            // Instruction.Jump
            // jump location (should be index of conditional)

            var loopBeginIdx = codeBlock.Size;
            codeBlock.Add(_conditionExpression.Generate(vm, scopeManager));
            codeBlock.Add(Instruction.JumpIfZero);

            var whileJumpIdx = codeBlock.Size;
            codeBlock.Add(0.0f); // dummy value

            _bodyStatement.EmitTo(codeBlock, vm, scopeManager);

            codeBlock.Add(Instruction.Jump);
            codeBlock.Add(loopBeginIdx);

            codeBlock[whileJumpIdx] = codeBlock.Size;
        }
    }
}