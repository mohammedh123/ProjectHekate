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
        /// <param name="propertyExpressions">The CLR properties mapped to the type's properties</param>
        /// <exception cref="System.ArgumentException">Thrown when a property with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when any of the expressions in  <paramref name="propertyExpressions"/> do not map to a CLR property.</exception>
        void AddProperty<TScriptObjectType>(string typeName, params Expression<Func<TScriptObjectType, float>>[] propertyExpressions) where TScriptObjectType : AbstractScriptObject;

        /// <summary>
        /// Retrieves the global property index of the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The global property index of the property with the given name if it has been registered.</returns>
        /// <exception cref="ArgumentException">Thrown when a property with a name of <paramref name="propertyName"/> has not been registered yet.</exception>
        int GetPropertyIndex(string propertyName);

        /// <summary>
        /// Loads code into the virtual machine.
        /// </summary>
        /// <param name="text">The code to be loaded.</param>
        void LoadCode(string text);

        /// <summary>
        /// Updates a script object.
        /// </summary>
        /// <typeparam name="TScriptObjectType">The type of script object being updated</typeparam>
        /// <param name="so">The script object being updated</param>
        void Update<TScriptObjectType>(TScriptObjectType so) where TScriptObjectType : AbstractScriptObject;

        /// <summary>
        /// Adds an external function to the virtual machine. External functions can be referenced in the script code and must be a function that takes in a single parameter of type <see cref="ScriptState"/> and returns a <seealso cref="ScriptStatus"/>.
        /// </summary>
        /// <param name="functionName">The name to use to refer to this function in the script</param>
        /// <param name="function">The function to add to the virtual machine</param>
        /// <exception cref="ArgumentException">Thrown when an external function with the name <paramref name="functionName"/> already exists.</exception>
        void AddExternalFunction(string functionName, Func<ScriptState, ScriptStatus> function);

        /// <summary>
        /// Adds a firing function to the virtual machine. Firing functions are used in code to fire bullets.
        /// </summary>
        /// <typeparam name="TFiringClass">The type of the object doing the firing (usually something like a bullet manager/system)</typeparam>
        /// <typeparam name="TBulletType">The type of the bullet being fired</typeparam>
        /// <param name="typeName">The name of the type this function fires (for use in the script)</param>
        /// <param name="functionName">The name of the firing function (for use in the script)</param>
        /// <param name="instance">An instance of <typeparamref name="TFiringClass"/> that will be used for firing bullets</param>
        /// <param name="methodSelector">A call to the firing method to add; use 0s, nulls, and default values to call the correct method</param>
        void AddFiringFunction<TFiringClass, TBulletType>(string typeName, string functionName, TFiringClass instance, Expression<Func<TFiringClass, TBulletType>> methodSelector);

        /// <summary>
        /// Gets a firing function associated with a type by its function name.
        /// </summary>
        /// <param name="typeName">The name of the type this function fires</param>
        /// <param name="functionName">The name of the firing function</param>
        /// <returns>Returns the appropriate <see cref="FiringFunctionDefinition"/> if a firing function for the given <paramref name="typeName"/> and with the given <paramref name="functionName"/> exists; <b>null</b> otherwise</returns>
        FiringFunctionDefinition GetFiringFunction(string typeName, string functionName);

        /// <summary>
        /// Gets a firing function by index.
        /// </summary>
        /// <param name="idx">The index into the list</param>
        /// <returns>Returns the appropriate <see cref="FiringFunctionDefinition"/> if a firing function at the given <paramref name="idx"/> exists; <b>null</b> otherwise</returns>
        FiringFunctionDefinition GetFiringFunctionByIndex(int idx);

        /// <summary>
        /// Gets an external function definition by its name.
        /// </summary>
        /// <param name="functionName">The name of the function</param>
        /// <returns>Returns the appropriate <see cref="FunctionDefinition"/> if a function with the name <paramref name="functionName"/> exists; <b>null</b> otherwise</returns>
        FunctionDefinition GetExternalFunction(string functionName);

        /// <summary>
        /// Gets an external function definition by its index.
        /// </summary>
        /// <param name="idx">The index of the function</param>
        /// <returns>Returns the appropriate <see cref="FunctionDefinition"/> if a function with the index <paramref name="idx"/> exists.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is greater or equal to the number of external functions registered or is less than 0.</exception>
        FunctionDefinition GetExternalFunctionByIndex(int idx);
    }
}