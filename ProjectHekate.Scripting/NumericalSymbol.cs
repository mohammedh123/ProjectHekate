using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class NumericalSymbol : ISymbol
    {
        public string Name { get; private set; }

        public SymbolTypes SymbolType
        {
            get { return SymbolTypes.Numerical; }
        }

        public NumericalSymbol(string name)
        {
            Name = name;
        }
    }
}