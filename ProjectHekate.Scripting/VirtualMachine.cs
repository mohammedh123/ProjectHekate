using System;
using System.Collections.Generic;
using System.Linq;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class VirtualMachine : IVirtualMachine
    {
        public const int MaxNumericalVariables = 64;
        public const int MaxEmitterVariables = 8;
        public const int MaxProperties = 32;
        
        private readonly List<FunctionCodeScope> _functionCodeScopes;
        private readonly List<BulletUpdaterCodeScope> _bulletUpdaterCodeScopes;
        private readonly List<EmitterUpdaterCodeScope> _emitterUpdaterCodeScopes;
        private readonly Dictionary<string, int> _functionCodeScopeNameToIndex;
        private readonly Dictionary<string, int> _bulletUpdaterCodeScopeNameToIndex;
        private readonly Dictionary<string, int> _emitterUpdaterCodeScopeNameToIndex;

        private readonly List<IdentifierRecord> _propertyRecords;
        private readonly Dictionary<string, int> _propertyNameToIndex;
        
        private readonly IDictionary<string, float> _globalSymbolsNameToValue;

        public VirtualMachine()
        {
            _functionCodeScopes = new List<FunctionCodeScope>();
            _functionCodeScopeNameToIndex = new Dictionary<string, int>();

            _bulletUpdaterCodeScopes = new List<BulletUpdaterCodeScope>();
            _bulletUpdaterCodeScopeNameToIndex = new Dictionary<string, int>();

            _emitterUpdaterCodeScopes = new List<EmitterUpdaterCodeScope>();
            _emitterUpdaterCodeScopeNameToIndex = new Dictionary<string, int>();

            _propertyRecords = new List<IdentifierRecord>();
            _propertyNameToIndex = new Dictionary<string, int>();

            _globalSymbolsNameToValue = new Dictionary<string, float>();
        }

        public int AddFunctionCodeScope(string name, FunctionCodeScope codeScope)
        {
            return AddSpecializedCodeScope(name, _functionCodeScopes, _functionCodeScopeNameToIndex, codeScope);
        }

        public FunctionCodeScope GetFunctionCodeScope(string name)
        {
            return GetSpecializedCodeScope(name, "function", _functionCodeScopes, _functionCodeScopeNameToIndex);
        }

        public int AddBulletUpdaterCodeScope(string name, BulletUpdaterCodeScope codeScope)
        {
            return AddSpecializedCodeScope(name, _bulletUpdaterCodeScopes, _bulletUpdaterCodeScopeNameToIndex, codeScope);
        }

        public BulletUpdaterCodeScope GetBulletUpdaterCodeScope(string name)
        {
            return GetSpecializedCodeScope(name, "bullet updater", _bulletUpdaterCodeScopes, _bulletUpdaterCodeScopeNameToIndex);
        }

        public int AddEmitterUpdaterCodeScope(string name, EmitterUpdaterCodeScope codeScope)
        {
            return AddSpecializedCodeScope(name, _emitterUpdaterCodeScopes, _emitterUpdaterCodeScopeNameToIndex, codeScope);
        }

        public EmitterUpdaterCodeScope GetEmitterUpdaterCodeScope(string name)
        {
            return GetSpecializedCodeScope(name, "emitter updater", _emitterUpdaterCodeScopes, _emitterUpdaterCodeScopeNameToIndex);
        }

        public int AddProperty(string name)
        {
            // TODO: make method thread-safe
            if (_propertyNameToIndex.ContainsKey(name))
                throw new ArgumentException("A property with the name \"" + name + "\" already exists in this virtual machine.", "name");

            var identifier = new IdentifierRecord(name, _propertyRecords.Count); // this is so bad and unsafe
            _propertyRecords.Add(identifier);
            _propertyNameToIndex[name] = identifier.Index;

            return identifier.Index;
        }

        public IdentifierRecord GetProperty(string name)
        {
            return GetSpecializedCodeScope(name, "property", _propertyRecords, _propertyNameToIndex);
        }

        private int AddSpecializedCodeScope<TCodeScopeType>(string name, ICollection<TCodeScopeType> codeScopeList, IDictionary<string,int> codeScopeNameToIndexMap, TCodeScopeType codeScope) where TCodeScopeType : CodeScope
        {
            // TODO: make method thread-safe
            ThrowIfCodeScopeWithNameAlreadyExists(name);
            ThrowIfCodeScopeAlreadyExists(codeScope);

            codeScopeList.Add(codeScope);
            codeScope.Index = codeScopeList.Count;
            codeScopeNameToIndexMap[name] = codeScope.Index;

            return codeScope.Index;
        }

        private TCodeScopeType GetSpecializedCodeScope<TCodeScopeType>(string name, string nameOfCodeScopeForExceptionMessage, IReadOnlyList<TCodeScopeType> specializedCodeScopes, Dictionary<string, int> specializedCodeScopeNameToIndex)
        {
            if(!specializedCodeScopeNameToIndex.ContainsKey(name))
                throw new ArgumentException("A " + nameOfCodeScopeForExceptionMessage + " with the name \"" + name + "\" could not be found.", name);

            return specializedCodeScopes[specializedCodeScopeNameToIndex[name]];
        }

        private void ThrowIfCodeScopeWithNameAlreadyExists(string name)
        {
            if (_functionCodeScopeNameToIndex.ContainsKey(name))
                throw new ArgumentException("A function with the name \"" + name + "\" already exists in this script.", "name");
            if (_bulletUpdaterCodeScopeNameToIndex.ContainsKey(name))
                throw new ArgumentException("A bullet updater with the name \"" + name + "\" already exists in this script.", "name");
            if (_emitterUpdaterCodeScopeNameToIndex.ContainsKey(name))
                throw new ArgumentException("An emitter updater with the name \"" + name + "\" already exists in this script.", "name");
        }

        private void ThrowIfCodeScopeAlreadyExists(CodeScope codeScope)
        {
            if (_functionCodeScopes.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as a function).", "codeScope");
            if (_bulletUpdaterCodeScopes.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as a bullet updater).", "codeScope");
            if (_emitterUpdaterCodeScopes.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as an emitter updater).", "codeScope");
        }

        public void AddGlobalSymbol(string name, float value)
        {
            if (_globalSymbolsNameToValue.ContainsKey(name))
                throw new ArgumentException("A global numerical symbol with the name \"" + name + "\" already exists.", "name");

            _globalSymbolsNameToValue[name] = value;
        }

        public bool TryGetGlobalSymbol(string name, out float value)
        {
            return _globalSymbolsNameToValue.TryGetValue(name, out value);
        }
    }
}
