using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class NumericalSymbol : ISymbol
    {
        private readonly string _name;
        private readonly int _index;

        public string Name { get { return _name; } }
        public int Index { get { return _index; } }

        public SymbolType Type
        {
            get { return SymbolType.Numerical; }
        }

        public NumericalSymbol(string name, int index)
        {
            _name = name;
            _index = index;
        }
    }
}