using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting
{
    enum Instructions
    {
        Push,
        Pop,
        OperatorNegative,
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
        Jump,
        Compare,
        FunctionCall,
        GetProperty,
        GetUpdater,
        GetVariable,
        SetVariable,
        Fire,
        WaitFrames
    }
}
