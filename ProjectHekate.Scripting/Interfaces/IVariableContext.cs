using System.Collections.Generic;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface IVariableContext
    {
        IReadOnlyList<IdentifierRecord> NumericalVariables { get; }
        IReadOnlyList<IdentifierRecord> EmitterVariables { get; }

        /// <summary>
        /// Adds a numerical variable to the code record.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the index of the identifier</returns>
        /// <exception cref="System.ArgumentException">Thrown when an identifier with that name already exists</exception>
        int AddNumericalVariable(string name);

        /// <summary>
        /// Gets the numerical variable with a given name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the identifier record belong to the numerical variable with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a variable with that name does not exist</exception>
        IdentifierRecord GetNumericalVariable(string name);

        /// <summary>
        /// Adds a emitter variable to the code record.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the index of the variable</returns>
        /// <exception cref="System.ArgumentException">Thrown when an variable with that name already exists</exception>
        int AddEmitterVariable(string name);

        /// <summary>
        /// Gets the emitter variable with a given name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the identifier record belonging to the emitter with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a variable with that name does not exist</exception>
        IdentifierRecord GetEmitterVariable(string name);
    }
}