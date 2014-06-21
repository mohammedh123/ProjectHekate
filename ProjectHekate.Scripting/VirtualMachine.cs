using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public interface ICodeBlock
    {
        /// <summary>
        /// The index of the codeblock in its enclosed collection.
        /// </summary>
        int Index { get; }

        int Size { get; }
        IReadOnlyList<float> Code { get; }
        float this[int idx] { get; set; }

        void Add(Instruction inst);
        void Add(byte b);
        void Add(ICodeBlock block);
        void Add(float f);
    }

    public interface IVariableContext
    {
        IReadOnlyList<IdentifierRecord> NumericalVariables { get; }
        IReadOnlyList<IdentifierRecord> EmitterVariables { get; }

        /// <summary>
        /// Adds a numerical variable to the code record.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the index of the identifier</returns>
        /// <exception cref="System.ArgumentException">Thrown when an identifier with that name already exists</exception>
        int AddNumericalVariable(string name);

        /// <summary>
        /// Gets the numerical variable with a given name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the identifier record belong to the numerical variable with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a variable with that name does not exist</exception>
        IdentifierRecord GetNumericalVariable(string name);

        /// <summary>
        /// Adds a emitter variable to the code record.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the index of the variable</returns>
        /// <exception cref="System.ArgumentException">Thrown when an variable with that name already exists</exception>
        int AddEmitterVariable(string name);

        /// <summary>
        /// Gets the emitter variable with a given name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the identifier record belonging to the emitter with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a variable with that name does not exist</exception>
        IdentifierRecord GetEmitterVariable(string name);
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

        private readonly List<FunctionCodeScope> _functionCodeBlocks;
        private readonly List<BulletUpdaterCodeScope> _bulletUpdaterCodeBlocks;
        private readonly List<EmitterUpdaterCodeScope> _emitterUpdaterCodeBlocks;
        private readonly List<IdentifierRecord> _propertyRecords;
        private readonly Dictionary<string, int> _functionCodeBlockNameToIndex;
        private readonly Dictionary<string, int> _bulletUpdaterCodeBlockNameToIndex;
        private readonly Dictionary<string, int> _emitterUpdaterCodeBlockNameToIndex;
        private readonly Dictionary<string, int> _propertyNameToIndex;

        public VirtualMachine()
        {
            _functionCodeBlocks = new List<FunctionCodeScope>();
            _functionCodeBlockNameToIndex = new Dictionary<string, int>();

            _bulletUpdaterCodeBlocks = new List<BulletUpdaterCodeScope>();
            _bulletUpdaterCodeBlockNameToIndex = new Dictionary<string, int>();

            _emitterUpdaterCodeBlocks = new List<EmitterUpdaterCodeScope>();
            _emitterUpdaterCodeBlockNameToIndex = new Dictionary<string, int>();

            _propertyRecords = new List<IdentifierRecord>();
            _propertyNameToIndex = new Dictionary<string, int>();

            FunctionCodeBlocks = _functionCodeBlocks.AsReadOnly();
            BulletUpdaterCodeBlocks = _bulletUpdaterCodeBlocks.AsReadOnly();
            EmitterUpdaterCodeBlocks = _emitterUpdaterCodeBlocks.AsReadOnly();
            PropertyRecords = _propertyRecords.AsReadOnly();
        }

        public int AddFunctionCodeBlock(string name, FunctionCodeScope codeScope)
        {
            return AddSpecializedCodeBlock(name, _functionCodeBlocks, _functionCodeBlockNameToIndex, codeScope);
        }

        public FunctionCodeScope GetFunctionCodeBlock(string name)
        {
            return GetSpecializedCodeBlock(name, "function", _functionCodeBlocks, _functionCodeBlockNameToIndex);
        }

        public int AddBulletUpdaterCodeBlock(string name, BulletUpdaterCodeScope codeScope)
        {
            return AddSpecializedCodeBlock(name, _bulletUpdaterCodeBlocks, _bulletUpdaterCodeBlockNameToIndex, codeScope);
        }

        public BulletUpdaterCodeScope GetBulletUpdaterCodeBlock(string name)
        {
            return GetSpecializedCodeBlock(name, "bullet updater", _bulletUpdaterCodeBlocks, _bulletUpdaterCodeBlockNameToIndex);
        }

        public int AddEmitterUpdaterCodeBlock(string name, EmitterUpdaterCodeScope codeScope)
        {
            return AddSpecializedCodeBlock(name, _emitterUpdaterCodeBlocks, _emitterUpdaterCodeBlockNameToIndex, codeScope);
        }

        public EmitterUpdaterCodeScope GetEmitterUpdaterCodeBlock(string name)
        {
            return GetSpecializedCodeBlock(name, "emitter updater", _emitterUpdaterCodeBlocks, _emitterUpdaterCodeBlockNameToIndex);
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
            return GetSpecializedCodeBlock(name, "property", _propertyRecords, _propertyNameToIndex);
        }

        private int AddSpecializedCodeBlock<TCodeBlockType>(string name, ICollection<TCodeBlockType> codeBlockList, IDictionary<string,int> codeBlockNameToIndexMap, TCodeBlockType codeBlock) where TCodeBlockType : CodeScope
        {
            // TODO: make method thread-safe
            ThrowIfCodeBlockWithNameAlreadyExists(name);
            ThrowIfCodeBlockAlreadyExists(codeBlock);

            codeBlockList.Add(codeBlock);
            codeBlock.Index = codeBlockList.Count;
            codeBlockNameToIndexMap[name] = codeBlock.Index;

            return codeBlock.Index;
        }

        private TCodeBlockType GetSpecializedCodeBlock<TCodeBlockType>(string name, string nameOfCodeBlockForExceptionMessage, IReadOnlyList<TCodeBlockType> specializedCodeBlocks, Dictionary<string, int> specializedCodeBlockNameToIndex)
        {
            if(!specializedCodeBlockNameToIndex.ContainsKey(name))
                throw new ArgumentException("A " + nameOfCodeBlockForExceptionMessage + " with the name \"" + name + "\" could not be found.", name);

            return specializedCodeBlocks[specializedCodeBlockNameToIndex[name]];
        }

        private void ThrowIfCodeBlockWithNameAlreadyExists(string name)
        {
            if (_functionCodeBlockNameToIndex.ContainsKey(name))
                throw new ArgumentException("A function with the name \"" + name + "\" already exists in this script.", "name");
            if (_bulletUpdaterCodeBlockNameToIndex.ContainsKey(name))
                throw new ArgumentException("A bullet updater with the name \"" + name + "\" already exists in this script.", "name");
            if (_emitterUpdaterCodeBlockNameToIndex.ContainsKey(name))
                throw new ArgumentException("An emitter updater with the name \"" + name + "\" already exists in this script.", "name");
        }

        private void ThrowIfCodeBlockAlreadyExists(CodeScope codeScope)
        {
            if (_functionCodeBlocks.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as a function).", "codeScope");
            if (_bulletUpdaterCodeBlocks.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as a bullet updater).", "codeScope");
            if (_emitterUpdaterCodeBlocks.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as an emitter updater).", "codeScope");
        }
    }
}
