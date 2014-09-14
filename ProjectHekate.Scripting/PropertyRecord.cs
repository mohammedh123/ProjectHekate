using System;

namespace ProjectHekate.Scripting
{
    public class PropertyRecord
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public Func<AbstractScriptObject, float> Getter { get; set; }
        public Action<AbstractScriptObject, float> Setter { get; set; }
    }
}