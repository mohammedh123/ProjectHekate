using System;

namespace ProjectHekate.Scripting
{
    public interface IPropertyRecord
    {
        string Name { get; set; }
        int Index { get; set; }
        float GetValue(AbstractScriptObject obj);
        void SetValue(AbstractScriptObject obj, float val);
    }

    public class PropertyRecord<TScriptObjectType> : IPropertyRecord where TScriptObjectType : AbstractScriptObject
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public float GetValue(AbstractScriptObject obj)
        {
            var castedObj = obj as TScriptObjectType;
            if(castedObj == null) throw new ArgumentException("Wrong script object type used for this property.");

            return Getter(castedObj);
        }

        public void SetValue(AbstractScriptObject obj, float val)
        {
            var castedObj = obj as TScriptObjectType;
            if (castedObj == null) throw new ArgumentException("Wrong script object type used for this property.");

            Setter(castedObj, val);
        }

        public Func<TScriptObjectType, float> Getter { get; set; }
        public Action<TScriptObjectType, float> Setter { get; set; }
    }
}