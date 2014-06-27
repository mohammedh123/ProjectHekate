namespace ProjectHekate.Scripting.Interfaces
{
    public interface ISymbol
    {
        string Name { get; }
        int Index { get; }
        SymbolType Type { get; }
    }
}