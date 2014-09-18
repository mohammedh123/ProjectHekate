using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Bytecode.Emitters
{
    public class ContinueStatementEmitter : EmptyEmitter
    {       
        private readonly IList<int> _continueList;

        public ContinueStatementEmitter(IList<int> continueList)
        {
            _continueList = continueList;
        }

        public override void EmitTo(ICodeBlock codeBlock, IVirtualMachine vm, IScopeManager scopeManager)
        {
            // Continue statement
            // Instruction.Jump
            // {0}, used as a dummy value; the enclosing loop construct must take care of replacing 
            //      the dummy value with the actual size of the construct's code scope
            // add a continue to the list of continues

            codeBlock.Add(Instruction.Jump);
            codeBlock.Add(0.0f); // dummy value

            _continueList.Add(codeBlock.Size-1); // index of the dummy value above
        }
    }
}
