﻿using System;
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
        OpAdd,
        OpSubtract,
        OpMultiply,
        OpDivide,
        OpMod,
        OpLessThan,
        OpLessThanEqual,
        OpGreaterThan,
        OpGreaterThanEqual,
        OpEqual,
        OpNotEqual,
        OpAnd,
        OpOr,
        OpNot,
        Jump,
        JumpOffset,
        IfZeroBranch,
        IfZeroBranchOffset,
        Return,
        ExternalFunctionCall,
        FunctionCall,
        GetUpdater,
        GetProperty,
        SetProperty,
        GetVariable,
        SetVariable,
        Fire,
        FireWithUpdater,
        WaitFrames
    }
}
