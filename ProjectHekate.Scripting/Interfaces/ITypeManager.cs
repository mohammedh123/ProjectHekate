namespace ProjectHekate.Scripting.Interfaces
{
    public interface ITypeManager
    {
        /// <summary>
        /// Adds a new scriptable type.
        /// </summary>
        /// <param name="typeName">The name of the new type</param>
        /// <returns>The index of the type</returns>
        int AddType<TScriptObjectType>(string typeName) where TScriptObjectType : AbstractScriptObject;

        /// <summary>
        /// Gets a scriptable type by name.
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        /// <returns>The type mapped to the type name; <b>null</b> if the type name doesn't exist</returns>
        ITypeDefinition GetType(string typeName);
    }
}