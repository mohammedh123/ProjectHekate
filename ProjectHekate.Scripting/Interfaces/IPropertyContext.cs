using System;
using System.Linq.Expressions;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface IPropertyContext
    {
        /// <summary>
        /// Adds a property to the type. A property is a float-type variable.
        /// </summary>
        /// <param name="propertyExpression">The CLR property mapped to the type property</param>
        /// <returns>Returns the index of the property</returns>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="propertyExpression"/> does not map to a CLR property.</exception>
        int AddProperty<TScriptObjectType>(Expression<Func<TScriptObjectType, float>> propertyExpression)
            where TScriptObjectType : AbstractScriptObject;

        /// <summary>
        /// Gets a property by name.
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <returns>Returns the identifier record of the property with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name does not exist</exception>
        IPropertyRecord GetProperty(string name);

        /// <summary>
        /// Gets a property by index.
        /// </summary>
        /// <param name="idx">The index of the property</param>
        /// <returns>Returns the identifier record of the property with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name does not exist</exception>
        IPropertyRecord GetProperty(int idx);
    }
}