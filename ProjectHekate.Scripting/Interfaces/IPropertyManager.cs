using System;
using System.Collections.Generic;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface IPropertyManager
    {
        IReadOnlyList<IdentifierRecord> PropertyRecords { get; }

        /// <summary>
        /// Adds a property to the virtual machine. A property is a float-type variable that belongs to all emitters.
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <returns>Returns the index of the property</returns>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name already exists</exception>
        int AddProperty(string name);

        /// <summary>
        /// Gets a property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the identifier record of the property with the given name</returns>
        /// <exception cref="ArgumentException">Thrown when a property with that name does not exist</exception>
        IdentifierRecord GetProperty(string name);
    }
}