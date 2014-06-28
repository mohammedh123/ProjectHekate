using System;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface ISymbolContext
    {
        /// <summary>
        /// Adds a new symbol to the code record.
        /// </summary>
        /// <returns>Returns the index of the symbol</returns>
        /// <exception cref="System.ArgumentException">Thrown when a symbol with that name already exists</exception>
        int AddSymbol(string name, SymbolType symbolType);

        /// <summary>
        /// Gets the symbol with a given name.
        /// </summary>
        /// <param name="name">The name of the symbol</param>
        /// <returns>Returns the symbol with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a symbol with that name does not exist</exception>
        ISymbol GetSymbol(string name);

        /// <summary>
        /// Returns whether or not a symbol with a given name has been defined in the context.
        /// </summary>
        /// <param name="name">The name of the symbol</param>
        /// <returns><b>true</b> if the symbol is defined in this scope; <b>false</b> otherwise.</returns>
        /// <exception cref="NullReferenceException">Thrown when <paramref name="name"/> is null</exception>
        bool HasSymbolDefined(string name);
    }
}