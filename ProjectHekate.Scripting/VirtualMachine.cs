using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public enum IdentifierType
    {
        Property,
        Variable
    }

    public struct IdentifierRecord
    {
        private readonly string _name;
        private readonly int _index;
        
        public string Name
        {
            get { return _name; }
        }

        public int Index
        {
            get { return _index; }
        }

        public IdentifierRecord(string name, int index)
        {
            _name = name;
            _index = index;
        }
    }

    public class CodeScope : ICodeBlock, IVariableContext
    {
        public int Index { get; set; }
        public int Size { get { return _code.Count; } }

        public IReadOnlyList<float> Code { get; private set; }
        private readonly List<float> _code;

        public IReadOnlyList<IdentifierRecord> NumericalVariables { get; private set; }
        private readonly List<IdentifierRecord> _numericalVariables;
        private readonly Dictionary<string, int> _numericalVariablesNameToIndex;

        public IReadOnlyList<IdentifierRecord> EmitterVariables { get; private set; }
        private readonly List<IdentifierRecord> _emitterVariables;
        private readonly Dictionary<string, int> _emitterVariablesNameToIndex;


        public float this[int idx]
        {
            get { return Code[idx]; }
            set { _code[idx] = value; }
        }

        public CodeScope()
        {
            _code = new List<float>();
            Code = _code.AsReadOnly();

            _numericalVariables = new List<IdentifierRecord>();
            _numericalVariablesNameToIndex = new Dictionary<string, int>();

            _emitterVariables = new List<IdentifierRecord>();
            _emitterVariablesNameToIndex = new Dictionary<string, int>();
            
            NumericalVariables = _numericalVariables.AsReadOnly();
            EmitterVariables = _emitterVariables.AsReadOnly();
        }

        public void Add(Instruction inst)
        {
            _code.Add((byte)inst);
        }

        public void Add(byte b)
        {
            _code.Add(b);
        }

        public void Add(ICodeBlock block)
        {
            if(block == null) throw new ArgumentNullException("block");

            _code.AddRange(block.Code);
        }

        public void Add(float f)
        {
            _code.Add(f);
        }

        public int AddNumericalVariable(string name)
        {
            return AddIdentifierToList(name, _numericalVariables, _numericalVariablesNameToIndex);
        }

        public IdentifierRecord GetNumericalVariable(string name)
        {
            return GetSpecificIdentifier(name, _numericalVariables, _numericalVariablesNameToIndex);
        }

        public int AddEmitterVariable(string name)
        {
            return AddIdentifierToList(name, _emitterVariables, _emitterVariablesNameToIndex);
        }

        public IdentifierRecord GetEmitterVariable(string name)
        {
            return GetSpecificIdentifier(name, _emitterVariables, _emitterVariablesNameToIndex);
        }

        private int AddIdentifierToList(string name, ICollection<IdentifierRecord> identifierList, IDictionary<string, int> identifierNameToIndexMap)
        {
            // TODO: make method thread-safe
            // TODO: make sure it checks all other identifiers too (emitters being made, final emitters, etc)
            if (_numericalVariablesNameToIndex.ContainsKey(name))
                throw new ArgumentException("An identifier with the name \"" + name + "\" already exists (as a numerical variable) in this script.", "name");

            var identifier = new IdentifierRecord(name, identifierList.Count); // this is so bad and unsafe
            identifierList.Add(identifier);
            identifierNameToIndexMap[name] = identifier.Index;

            return identifier.Index;
        }

        private IdentifierRecord GetSpecificIdentifier(string name, IList<IdentifierRecord> identifierList, IDictionary<string, int> identifierNameToIndexMap)
        {
            int index;
            var worked = identifierNameToIndexMap.TryGetValue(name, out index);

            if (!worked) throw new ArgumentException("An identifier with the name " + name + " could not be found.");

            return identifierList[index];
        }
    }
    
    public class FunctionCodeScope : CodeScope
    {
        public FunctionCodeScope(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddNumericalVariable(paramName);
            }
        }
    }

    public class BulletUpdaterCodeScope : CodeScope
    {
        public BulletUpdaterCodeScope(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddNumericalVariable(paramName);
            }
        }
    }

    public class EmitterUpdaterCodeScope : CodeScope
    {
        public EmitterUpdaterCodeScope(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddNumericalVariable(paramName);
            }
        }
    }

    public class VirtualMachine : IVirtualMachine
    {
        public const int MaxNumericalVariables = 64;
        public const int MaxEmitterVariables = 8;
        public const int MaxProperties = 32;


        public IReadOnlyList<FunctionCodeScope> FunctionCodeBlocks { get; private set; }
        public IReadOnlyList<BulletUpdaterCodeScope> BulletUpdaterCodeBlocks { get; private set; }
        public IReadOnlyList<EmitterUpdaterCodeScope> EmitterUpdaterCodeBlocks { get; private set; }
        public IReadOnlyList<IdentifierRecord> PropertyRecords { get; private set; }
        public CodeScope CurrentCode { get; set; }

        private readonly List<FunctionCodeScope> _functionCodeScopes;
        private readonly List<BulletUpdaterCodeScope> _bulletUpdaterCodeScopes;
        private readonly List<EmitterUpdaterCodeScope> _emitterUpdaterCodeScopes;
        private readonly List<IdentifierRecord> _propertyRecords;
        private readonly Dictionary<string, int> _functionCodeScopeNameToIndex;
        private readonly Dictionary<string, int> _bulletUpdaterCodeScopeNameToIndex;
        private readonly Dictionary<string, int> _emitterUpdaterCodeScopeNameToIndex;
        private readonly Dictionary<string, int> _propertyNameToIndex;

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

            FunctionCodeBlocks = _functionCodeScopes.AsReadOnly();
            BulletUpdaterCodeBlocks = _bulletUpdaterCodeScopes.AsReadOnly();
            EmitterUpdaterCodeBlocks = _emitterUpdaterCodeScopes.AsReadOnly();
            PropertyRecords = _propertyRecords.AsReadOnly();
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
    }
}
