using System.Collections.Generic;

namespace ProjectHekate.Scripting.Interfaces
{
    public interface ITypeDefinition : IPropertyContext
    {
        string Name { get; set; }
        int Index { get; set; }
        IPropertyRecord GetPropertyByGlobalIndex(int globalIdx);
        void UpdatePropertyMappings(IList<string> properties);
    }
}