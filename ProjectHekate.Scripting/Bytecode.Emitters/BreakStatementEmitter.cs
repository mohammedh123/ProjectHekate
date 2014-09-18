using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class BreakStatementEmitter : EmptyEmitter
    {
        private readonly IList<int> _breakList;

        public BreakStatementEmitter(IList<int> breakList)
        {
            _breakList = breakList;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Break statement
            // Instruction.Jump
            // {0}, used as a dummy value; the enclosing loop construct must take care of replacing 
            //      the dummy value with the actual size of the construct's code scope
            // add a break to the list of breaks

            codeBlock.Add(Instruction.Jump);
            codeBlock.Add(0.0f); // dummy value

            _breakList.Add(codeBlock.Size-1); // index of the dummy value above
        }
    }
}
