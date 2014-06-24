namespace ProjectHekate.Scripting.Interfaces
{
    public interface ISymbol
    {
        string Name { get; }

        SymbolTypes SymbolType { get; }
    }
}