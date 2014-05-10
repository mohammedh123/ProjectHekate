using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Grammar.Implementation
{
    public class BytecodeCompiler
    {
        public void Generate(HekateParser.ExpressionStatementContext context)
        {
            
        }
    }

    class BytecodeBlock
    {
        public List<uint> Bytes { get; set; }

        public BytecodeBlock()
        {
            Bytes = new List<uint>();
        }
    }
}
