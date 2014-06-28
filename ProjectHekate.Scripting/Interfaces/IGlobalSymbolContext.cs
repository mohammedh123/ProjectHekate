using System;

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
        /// Gets the value for a global symbol with a given name.
        /// </summary>
        /// <param name="name">The name of the global symbol</param>
        /// <returns>Returns the value of the global symbol with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a global symbol with that name does not exist</exception>
        float GetGlobalSymbolValue(string name);

        /// <summary>
        /// Returns whether or not a global symbol with a given name has been added.
        /// </summary>
        /// <param name="name">The name of the global symbol</param>
        /// <returns><b>true</b> if the global symbol is defined in this scope; <b>false</b> otherwise.</returns>
        /// <exception cref="NullReferenceException">Thrown when <paramref name="name"/> is null</exception>
        bool HasGlobalSymbolDefined(string name);
    }
}