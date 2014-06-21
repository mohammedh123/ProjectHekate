using System.Collections.Generic;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface ICodeBlock
    {
        /// <summary>
        /// The index of the codeblock in its enclosed collection.
        /// </summary>
        int Index { get; }

        int Size { get; }
        IReadOnlyList<float> Code { get; }
        float this[int idx] { get; set; }

        void Add(Instruction inst);
        void Add(byte b);
        void Add(ICodeBlock block);
        void Add(float f);
    }
}