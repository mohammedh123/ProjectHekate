using System;
using System.Linq.Expressions;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface IVirtualMachine : IGlobalSymbolContext, ITypeManager
    {
        /// <summary>
        /// Adds a function code scope to the virtual machine.
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="codeScope">The function code scope</param>
        /// <returns>Returns the index of the function code scope (also populates the Index property of the code scope)</returns>
        /// <exception cref="System.ArgumentException">Thrown when a function with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the function has already been added, but with a different name</exception>
        int AddFunctionCodeScope(string name, FunctionCodeScope codeScope);

        /// <summary>
        /// Gets a function code scope by name if it exists.
        /// </summary>
        /// <param name="name">The name of the function code scope</param>
        /// <returns>The function code scope mapped with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a function with that name does not exist</exception>
        FunctionCodeScope GetFunctionCodeScope(string name);

        /// <summary>
        /// Adds an action code scope to the virtual machine.
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="codeScope">The action code scope</param>
        /// <returns>Returns the index of the action code scope (also populates the Index property of the code scope)</returns>
        /// <exception cref="System.ArgumentException">Thrown when an action with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the action has already been added, but with a different name</exception>
        int AddActionCodeScope(string name, ActionCodeScope codeScope);

        /// <summary>
        /// Gets an action code scope by name if it exists.
        /// </summary>
        /// <param name="name">The name of the action code scope</param>
        /// <returns>The action code scope mapped with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when an action with that name does not exist</exception>
        ActionCodeScope GetActionCodeScope(string name);

        /// <summary>
        /// Adds an emitter updater code scope to the program code scope.
        /// </summary>
        /// <param name="name">The name of the emitter updater</param>
        /// <param name="codeScope">The code scope for the emitter updater</param>
        /// <returns>Returns the index of the emitter updater code scope (also populates the Index property of the CodeScope)</returns>
        /// <exception cref="System.ArgumentException">Thrown when a emitter updater with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the emitter updater has already been added, but with a different name</exception>
        int AddEmitterUpdaterCodeScope(string name, EmitterUpdaterCodeScope codeScope);

        /// <summary>
        /// Gets a emitter updater code scope by name if it exists.
        /// </summary>
        /// <param name="name">The name of the emitter updater code scope</param>
        /// <returns>The emitter updater code scope mapped with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a emitter updater with that name does not exist</exception>
        EmitterUpdaterCodeScope GetEmitterUpdaterCodeScope(string name);

        /// <summary>
        /// Adds a property to the virtual machine. A property is a float-type variable that belongs to all emitters.
        /// </summary>
        /// <param name="typeName">The name of the type this property belongs to</param>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyExpression">The CLR property mapped to the type property</param>
        /// <returns>Returns the index of the property</returns>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="propertyExpression"/> does not map to a CLR property.</exception>
        int AddProperty(string typeName, string propertyName, Expression<Func<AbstractScriptObject, float>> propertyExpression);

        /// <summary>
        /// Retrieves the global property index of the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The global property index of the property with the given name if it has been registered; <b>-1></b> otherwise.</returns>
        int GetPropertyIndex(string propertyName);

        void LoadCode(string text);

        void Update(AbstractScriptObject so, float delta);
    }
}