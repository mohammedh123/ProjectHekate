using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class TypeDefinition : ITypeDefinition
    {
        public string Name { get; set; }

        public int Index { get; set; }

        private readonly List<IPropertyRecord> _propertyDefinitions;
        private readonly Dictionary<string, int> _propertyDefinitionsNameToIndex;

        private readonly IList<int> _globalPropertyIndexMappings;  

        public TypeDefinition(string name, int index)
        {
            Name = name;
            Index = index;

            _propertyDefinitions = new List<IPropertyRecord>();
            _propertyDefinitionsNameToIndex = new Dictionary<string, int>();
            _globalPropertyIndexMappings = new List<int>();
        }

        public int AddProperty<TScriptObjectType, TPropertyType>(Expression<Func<TScriptObjectType, TPropertyType>> propertyExpression) where TScriptObjectType : AbstractScriptObject
        {
            var pi = GetPropertyInfoAndThrowIfExpressionIsntProperty(propertyExpression);

            return AddProperty(propertyExpression, pi.Name);
        }

        public int AddProperty<TScriptObjectType, TPropertyType>(Expression<Func<TScriptObjectType, TPropertyType>> propertyExpression, string name) where TScriptObjectType : AbstractScriptObject
        {
            var pi = GetPropertyInfoAndThrowIfExpressionIsntProperty(propertyExpression);
            var propertyName = name;

            int index;
            var worked = _propertyDefinitionsNameToIndex.TryGetValue(propertyName, out index);

            if (worked) throw new ArgumentException("A property with the name " + propertyName + " already exists.");

            var addedProperty = new PropertyRecord<TScriptObjectType>();

            var setMethod = pi.GetSetMethod();
            var getMethod = pi.GetGetMethod();

            var target = Expression.Parameter(typeof(TScriptObjectType));
            var value = Expression.Parameter(typeof(float));
            MethodCallExpression setBody;
            Action<TScriptObjectType, float> setMethodAction;
            if (typeof (TPropertyType) == typeof (bool)) {
                var equalityValue = Expression.Equal(value, Expression.Constant(1.0f));

                // if its a bool, it should be
                // void SetProperty(float value)
                //      this.Property = value == 1.0f;
                setBody = Expression.Call(target, setMethod, equalityValue);
                setMethodAction = Expression.Lambda<Action<TScriptObjectType, float>>(setBody, target, value).Compile();
            }
            else {
                setBody = Expression.Call(target, setMethod, Expression.Convert(value, typeof (TPropertyType)));
                // void SetProperty(float value)
                //      this.Property = (TPropertyType)value;
                setMethodAction = Expression.Lambda<Action<TScriptObjectType, float>>(setBody, target, value).Compile();
            }

            var callBody = Expression.Call(target, getMethod);
            var getBody = Expression.Convert(callBody, typeof(float));
            var getMethodAction = Expression.Lambda<Func<TScriptObjectType, float>>(getBody, target).Compile();

            addedProperty.Name = propertyName;
            addedProperty.Index = _propertyDefinitions.Count;
            addedProperty.Getter = getMethodAction;
            addedProperty.Setter = setMethodAction;

            _propertyDefinitionsNameToIndex[propertyName] = addedProperty.Index;
            _propertyDefinitions.Add(addedProperty);

            return addedProperty.Index;
        }

        public IPropertyRecord GetProperty(string name)
        {
            if (!_propertyDefinitionsNameToIndex.ContainsKey(name))
                throw new ArgumentException("A property with the name \"" + name + "\" could not be found.", name);

            return _propertyDefinitions[_propertyDefinitionsNameToIndex[name]];
        }

        public IPropertyRecord GetProperty(int idx)
        {
            return _propertyDefinitions[idx];
        }

        public IPropertyRecord GetPropertyByGlobalIndex(int globalIdx)
        {
            var idx = _globalPropertyIndexMappings[globalIdx];

            return _propertyDefinitions[idx];
        }

        private PropertyInfo GetPropertyInfoAndThrowIfExpressionIsntProperty<TScriptObjectType, TType>(Expression<Func<TScriptObjectType, TType>> propertyExpression)
            where TScriptObjectType : AbstractScriptObject
        {
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