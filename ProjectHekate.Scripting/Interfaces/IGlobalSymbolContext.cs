namespace ProjectHekate.Scripting.Interfaces
{
    public interface IGlobalSymbolContext
    {
        /// <summary>
        /// Adds a new global numerical symbol to the code record.
        /// </summary>
        /// <returns>Returns the index of the symbol</returns>
        /// <exception cref="System.ArgumentException">Thrown when a symbol with that name already exists</exception>
        void AddGlobalSymbol(string name, float value);

        /// <summary>
        /// Gets the global numerical symbol with a given name.
        /// </summary>
        /// <param name="name">The name of the global numerical symbol</param>
        /// <param name="value">The output parameter for the symbol's value</param>
        /// <returns>Returns whether or not a global numerical symbol exists with that name</returns>
        bool TryGetGlobalSymbol(string name, out float value);
    }
}