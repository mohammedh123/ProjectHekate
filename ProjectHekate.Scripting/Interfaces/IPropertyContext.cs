using System;
using System.Linq.Expressions;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface IPropertyContext
    {
        /// <summary>
        /// Adds a property to the type. A property is a float-type variable.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyExpression">The CLR property mapped to the type property</param>
        /// <returns>Returns the index of the property</returns>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="propertyExpression"/> does not map to a CLR property.</exception>
        int AddProperty(string propertyName, Expression<Func<AbstractScriptObject, float>> propertyExpression);

        /// <summary>
        /// Gets a property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns the identifier record of the property with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name does not exist</exception>
        PropertyRecord GetProperty(string name);
    }
}