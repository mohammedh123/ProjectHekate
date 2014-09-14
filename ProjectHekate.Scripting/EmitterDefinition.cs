using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class EmitterDefinition : TypeDefinition
    {
        public EmitterDefinition(string name, int index) : base(name, index)
        {
        }
    }
    
    public class TypeDefinition : IPropertyContext
    {
        public string Name { get; set; }

        public int Index { get; set; }

        private readonly List<PropertyRecord> _propertyDefinitions;
        private readonly Dictionary<string, int> _propertyDefinitionsNameToIndex;

        private readonly IList<int> _globalPropertyIndexMappings;  

        public TypeDefinition(string name, int index)
        {
            Name = name;
            Index = index;

            _propertyDefinitions = new List<PropertyRecord>();
            _propertyDefinitionsNameToIndex = new Dictionary<string, int>();
            _globalPropertyIndexMappings = new List<int>();
        }

        public int AddProperty(string propertyName, Expression<Func<AbstractScriptObject, float>> propertyExpression)
        {
            int index;
            var worked = _propertyDefinitionsNameToIndex.TryGetValue(propertyName, out index);

            if(worked) throw new ArgumentException("A property with the name " + propertyName + " already exists.");

            var addedProperty = new PropertyRecord();

            var pi = GetPropertyInfoAndThrowIfExpressionIsntProperty(propertyExpression);
            var setMethod = pi.GetSetMethod();
            var getMethod = pi.GetGetMethod();

            var target = Expression.Parameter(typeof(AbstractScriptObject));
            var value = Expression.Parameter(typeof(float));
            var setBody = Expression.Call(target, setMethod, Expression.Convert(value, pi.PropertyType));
            var setMethodAction = Expression.Lambda<Action<AbstractScriptObject, float>>(setBody, target, value).Compile();
            var getBody = Expression.Call(target, getMethod);
            var getMethodAction = Expression.Lambda<Func<AbstractScriptObject, float>>(getBody, target).Compile();

            addedProperty.Name = propertyName;
            addedProperty.Index = _propertyDefinitions.Count;
            addedProperty.Getter = getMethodAction;
            addedProperty.Setter = setMethodAction;

            _propertyDefinitionsNameToIndex[propertyName] = addedProperty.Index;
            _propertyDefinitions.Add(addedProperty);

            return addedProperty.Index;
        }

        public PropertyRecord GetProperty(string name)
        {
            if (!_propertyDefinitionsNameToIndex.ContainsKey(name))
                throw new ArgumentException("A property with the name \"" + name + "\" could not be found.", name);

            return _propertyDefinitions[_propertyDefinitionsNameToIndex[name]];
        }

        public PropertyRecord GetPropertyByGlobalIndex(int globalIdx)
        {
            var idx = _globalPropertyIndexMappings[globalIdx];

            return _propertyDefinitions[idx];
        }

        private PropertyInfo GetPropertyInfoAndThrowIfExpressionIsntProperty<TScriptObjectType>(Expression<Func<TScriptObjectType, float>> propertyExpression)
            where TScriptObjectType : AbstractScriptObject
        {
            var type = typeof (TScriptObjectType);

            var member = propertyExpression.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(String.Format("Expression '{0}' refers to a method, not a property.", propertyExpression));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(String.Format("Expression '{0}' refers to a field, not a property.", propertyExpression));

            //if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            //    throw new ArgumentException(String.Format("Expression '{0}' refers to a property that is not from type {1}.", propertyExpression, type));

            return propInfo;
        }

        public void UpdatePropertyMappings(IList<string> properties)
        {
            _globalPropertyIndexMappings.Clear();

            for (var i = 0; i < _propertyDefinitions.Count; i++) {
                var property = _propertyDefinitions[i];
                _globalPropertyIndexMappings.Add(-1);

                for (var j = 0; j < properties.Count; j++) {

                    if (property.Name == properties[j]) {
                        _globalPropertyIndexMappings[i] = j;
                        break;
                    }
                }
            }
        }
    }
}
