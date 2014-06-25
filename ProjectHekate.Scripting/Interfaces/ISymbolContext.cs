namespace ProjectHekate.Scripting.Interfaces
{
    public interface ISymbolContext
    {
        /// <summary>
        /// Adds a new symbol to the code record.
        /// </summary>
        /// <returns>Returns the index of the symbol</returns>
        /// <exception cref="System.ArgumentException">Thrown when a symbol with that name already exists</exception>
        int AddSymbol(string name, SymbolTypes symbolType);

        /// <summary>
        /// Gets the symbol with a given name.
        /// </summary>
        /// <param name="name">The name of the symbol</param>
        /// <returns>Returns the symbol with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a symbol with that name does not exist</exception>
        ISymbol GetSymbol(string name);
    }
}