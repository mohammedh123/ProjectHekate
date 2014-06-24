using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class EmitterSymbol : ISymbol
    {
        public string Name { get; private set; }

        public SymbolTypes SymbolType
        {
            get { return SymbolTypes.Emitter; }
        }

        public EmitterSymbol(string name)
        {
            Name = name;
        }
    }
}