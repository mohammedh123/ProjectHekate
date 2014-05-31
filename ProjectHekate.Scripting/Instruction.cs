using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting
{
    public enum Instruction : byte
    {
        Push,
        Pop,
        Negate,
        OperatorAdd,
        OperatorSubtract,
        OperatorMultiply,
        OperatorDivide,
        OperatorMod,
        OperatorLessThan,
        OperatorLessThanEqual,
        OperatorGreaterThan,
        OperatorGreaterThanEqual,
        OperatorEqual,
        OperatorNotEqual,
        OperatorAnd,
        OperatorOr,
        OperatorNot,
        Jump,
        Compare,
        FunctionCall,
        GetUpdater,
        GetProperty,
        SetProperty,
        GetVariable,
        SetVariable,
        Fire,
        WaitFrames
    }
}
