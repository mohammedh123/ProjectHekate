using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting
{
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

    public class CodeBlock
    {
        public int Index { get; set;  }

        public IReadOnlyList<uint> Code { get; private set; }
        private readonly List<uint> _code;

        public IReadOnlyList<IdentifierRecord> NumericalVariables { get; private set; }
        private readonly List<IdentifierRecord> _numericalVariables;
        private readonly Dictionary<string, int> _numericalVariablesNameToIndex;

        public IReadOnlyList<IdentifierRecord> EmitterVariables { get; private set; }
        private readonly List<IdentifierRecord> _emitterVariables;
        private readonly Dictionary<string, int> _emitterVariablesNameToIndex;
        
        public CodeBlock()
        {
            _code = new List<uint>();
            Code = _code.AsReadOnly();

            _numericalVariables = new List<IdentifierRecord>();
            _numericalVariablesNameToIndex = new Dictionary<string, int>();

            _emitterVariables = new List<IdentifierRecord>();
            _emitterVariablesNameToIndex = new Dictionary<string, int>();
            
            NumericalVariables = _numericalVariables.AsReadOnly();
            EmitterVariables = _emitterVariables.AsReadOnly();
        }

        public void Add(Instructions inst)
        {
            _code.Add((byte)inst);
        }

        public void Add(byte b)
        {
            _code.Add(b);
        }

        public void Add(CodeBlock block)
        {
            if(block != null)
                _code.AddRange(block.Code);
        }

        /// <summary>
        /// Adds a numerical variable to the code record.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the index of the identifier</returns>
        /// <exception cref="System.ArgumentException">Thrown when an identifier with that name already exists</exception>
        public int AddNumericalVariable(string name)
        {
            return AddIdentifierToList(name, _numericalVariables, _numericalVariablesNameToIndex);
        }

        /// <summary>
        /// Gets the numerical variable with a given name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the identifier record belong to the numerical variable with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a variable with that name does not exist</exception>
        public IdentifierRecord GetNumericalVariable(string name)
        {
            return GetSpecificIdentifier(name, _numericalVariables, _numericalVariablesNameToIndex);
        }

        /// <summary>
        /// Adds a emitter variable to the code record.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the index of the variable</returns>
        /// <exception cref="System.ArgumentException">Thrown when an variable with that name already exists</exception>
        public int AddEmitterVariable(string name)
        {
            return AddIdentifierToList(name, _emitterVariables, _emitterVariablesNameToIndex);
        }

        /// <summary>
        /// Gets the emitter variable with a given name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>Returns the identifier record belonging to the emitter with the given name</returns>
        /// <exception cref="System.ArgumentException">Thrown when a variable with that name does not exist</exception>
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

            var identifier = new IdentifierRecord(name, identifierList.Count + 1); // this is so bad and unsafe
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

    // TODO: do i even _NEED_ this SHIT?
    public class FunctionCodeBlock : CodeBlock
    {
        public FunctionCodeBlock(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddNumericalVariable(paramName);
            }
        }
    }

    // TODO: do i even _NEED_ this SHIT?
    public class BulletUpdaterCodeBlock : CodeBlock
    {
        public BulletUpdaterCodeBlock(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddNumericalVariable(paramName);
            }
        }
    }

    // TODO: do i even _NEED_ this SHIT?
    public class EmitterUpdaterCodeBlock : CodeBlock
    {
        public EmitterUpdaterCodeBlock(IEnumerable<string> paramNames)
        {
            // the parameters are added as local variables
            foreach(var paramName in paramNames) {
                AddNumericalVariable(paramName);
            }
        }
    }

    public class ScriptState
    {
        public int CurrentInstructionIndex { get; set; }
        public int CodeBlockIndex { get; set; }

        public IReadOnlyList<float> NumericalVariables { get; set; }
        public IReadOnlyList<object> EmitterVariables { get; set; } // TODO: change this to SOMETHING else
        public IReadOnlyList<float> Properties { get; set; }  

        private float[] _numericalVariables;
        private object[] _emitterVariables; // TODO: change this to SOMETHING else
        private float[] _properties;

        public ScriptState()
        {
            _numericalVariables = new float[VirtualMachine.MaxNumericalVariables];
            _emitterVariables = new object[VirtualMachine.MaxEmitterVariables];
            _properties = new float[VirtualMachine.MaxProperties];

            NumericalVariables = Array.AsReadOnly(_numericalVariables);
            EmitterVariables = Array.AsReadOnly(_emitterVariables);
            Properties = Array.AsReadOnly(_properties);
        }
    }

    public class VirtualMachine
    {
        public const int MaxNumericalVariables = 64;
        public const int MaxEmitterVariables = 8;
        public const int MaxProperties = 32;


        public IReadOnlyList<FunctionCodeBlock> FunctionCodeBlocks { get; private set; }
        public IReadOnlyList<BulletUpdaterCodeBlock> BulletUpdaterCodeBlocks { get; private set; }
        public IReadOnlyList<EmitterUpdaterCodeBlock> EmitterUpdaterCodeBlocks { get; private set; }

        private readonly List<FunctionCodeBlock> _functionCodeBlocks;
        private readonly List<BulletUpdaterCodeBlock> _bulletUpdaterCodeBlocks;
        private readonly List<EmitterUpdaterCodeBlock> _emitterUpdaterCodeBlocks;
        private readonly Dictionary<string, int> _functionCodeBlockNameToIndex;
        private readonly Dictionary<string, int> _bulletUpdaterCodeBlockNameToIndex;
        private readonly Dictionary<string, int> _emitterUpdaterCodeBlockNameToIndex;

        public VirtualMachine()
        {
            _functionCodeBlocks = new List<FunctionCodeBlock>();
            _functionCodeBlockNameToIndex = new Dictionary<string, int>();

            _bulletUpdaterCodeBlocks = new List<BulletUpdaterCodeBlock>();
            _bulletUpdaterCodeBlockNameToIndex = new Dictionary<string, int>();

            _emitterUpdaterCodeBlocks = new List<EmitterUpdaterCodeBlock>();
            _emitterUpdaterCodeBlockNameToIndex = new Dictionary<string, int>();

            BulletUpdaterCodeBlocks = _bulletUpdaterCodeBlocks.AsReadOnly();
            EmitterUpdaterCodeBlocks = _emitterUpdaterCodeBlocks.AsReadOnly();
        }

        /// <summary>
        /// Adds a function code block to the virtual machine.
        /// </summary>
        /// <param name="name">The name of the bullet updater</param>
        /// <param name="codeBlock">The function code block</param>
        /// <returns>Returns the index of the function code block (also populates the Index property of the code block)</returns>
        /// <exception cref="System.ArgumentException">Thrown when a function with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the function has already been added, but with a different name</exception>
        public int AddFunctionCodeBlock(string name, FunctionCodeBlock codeBlock)
        {
            return AddSpecializedCodeBlock(name, _functionCodeBlocks, _functionCodeBlockNameToIndex, codeBlock);
        }

        /// <summary>
        /// Adds a bullet updater code block to the virtual machine.
        /// </summary>
        /// <param name="name">The name of the bullet updater</param>
        /// <param name="codeBlock">The bullet updater code block</param>
        /// <returns>Returns the index of the bullet updater code block (also populates the Index property of the code block)</returns>
        /// <exception cref="System.ArgumentException">Thrown when a bullet updater with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the bullet updater has already been added, but with a different name</exception>
        public int AddBulletUpdaterCodeBlock(string name, BulletUpdaterCodeBlock codeBlock)
        {
            return AddSpecializedCodeBlock(name, _bulletUpdaterCodeBlocks, _bulletUpdaterCodeBlockNameToIndex, codeBlock);
        }

        /// <summary>
        /// Adds an emitter updater code block to the program code block.
        /// </summary>
        /// <param name="name">The name of the emitter updater</param>
        /// <param name="codeBlock">The code block for the emitter updater</param>
        /// <returns>Returns the index of the emitter updater code block (also populates the Index property of the codeBlock)</returns>
        /// <exception cref="System.ArgumentException">Thrown when a emitter updater with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the emitter updater has already been added, but with a different name</exception>
        public int AddEmitterUpdaterCodeBlock(string name, EmitterUpdaterCodeBlock codeBlock)
        {
            return AddSpecializedCodeBlock(name, _emitterUpdaterCodeBlocks, _emitterUpdaterCodeBlockNameToIndex, codeBlock);
        }

        private int AddSpecializedCodeBlock<TCodeBlockType>(string name, ICollection<TCodeBlockType> codeBlockList, IDictionary<string,int> codeBlockNameToIndexMap, TCodeBlockType codeBlock) where TCodeBlockType : CodeBlock
        {
            // TODO: make method thread-safe
            ThrowIfCodeBlockWithNameAlreadyExists(name);
            ThrowIfCodeBlockAlreadyExists(codeBlock);

            codeBlockList.Add(codeBlock);
            codeBlock.Index = codeBlockList.Count;
            codeBlockNameToIndexMap[name] = codeBlock.Index;

            return codeBlock.Index;
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

        private void ThrowIfCodeBlockAlreadyExists(CodeBlock codeBlock)
        {
            if (_functionCodeBlocks.Contains(codeBlock))
                throw new ArgumentException("This code block has already been added, but with a different name (as a function).", "codeBlock");
            if (_bulletUpdaterCodeBlocks.Contains(codeBlock))
                throw new ArgumentException("This code block has already been added, but with a different name (as a bullet updater).", "codeBlock");
            if (_emitterUpdaterCodeBlocks.Contains(codeBlock))
                throw new ArgumentException("This code block has already been added, but with a different name (as an emitter updater).", "codeBlock");
        }
    }
}
