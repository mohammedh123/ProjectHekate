using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class CodeBlock : ICodeBlock
    {
        public int Index { get; set; }
        public int Size { get { return _code.Count; } }
        public IReadOnlyList<float> Code { get; private set; }
        private readonly List<float> _code;

        public float this[int idx]
        {
            get { return Code[idx]; }
            set { _code[idx] = value; }
        }

        public CodeBlock()
        {
            _code = new List<float>();
            Code = _code.AsReadOnly();
        }

        public void Add(Instruction inst)
        {
            _code.Add((byte)inst);
        }

        public void Add(byte b)
        {
            _code.Add(b);
        }

        public void Add(ICodeBlock block)
        {
            if (block == null) throw new ArgumentNullException("block");

            _code.AddRange(block.Code);
        }

        public void Add(float f)
        {
            _code.Add(f);
        }
    }
}
