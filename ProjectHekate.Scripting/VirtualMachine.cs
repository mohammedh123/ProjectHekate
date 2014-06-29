using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class VirtualMachine : IVirtualMachine, IBytecodeInterpreter
    {
        private const float TrueValue = 1.0f;
        private const float FalseValue = 0.0f;

        public const int MaxNumericalVariables = 64;
        public const int MaxEmitterVariables = 8;
        public const int MaxProperties = 32;
        public const int MaxStackSize = 64;
        
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

        public float GetGlobalSymbolValue(string name)
        {
            float val;
            var worked = _globalSymbolsNameToValue.TryGetValue(name, out val);

            if (!worked) throw new ArgumentException("A global numerical symbol with the name " + name + " could not be found.");

            return val;
        }

        public bool HasGlobalSymbolDefined(string name)
        {
            return _globalSymbolsNameToValue.ContainsKey(name);
        }

        public ScriptStatus InterpretCode(ICodeBlock code, ScriptState state, bool looping)
        {
            if (code == null) throw new ArgumentNullException("code");
            if (state == null) throw new ArgumentNullException("state");
            if (state.CurrentInstructionIndex >= code.Size) return ScriptStatus.Ok;


            while (true) {
                var inst = (Instruction)code[state.CurrentInstructionIndex];

                switch (inst) {
                    case Instruction.Push:
                    {
                        state.Stack[state.StackHead] = code[state.CurrentInstructionIndex + 1];
                        state.StackHead++;
                        state.CurrentInstructionIndex += 2;
                        
                        ThrowIfStackLimitIsReached(state);
                        break;
                    }
                    case Instruction.Pop:
                    {
                        state.StackHead--;
                        state.CurrentInstructionIndex += 1;

                        ThrowIfStackBaseIsHit(state);
                        break;   
                    }
                    case Instruction.Negate:
                    {
                        ThrowIfStackIsEmpty(state);

                        state.Stack[state.StackHead - 1] *= -1;
                        state.CurrentInstructionIndex += 1;

                        break;
                    }
                    case Instruction.OpNot:
                    {
                        ThrowIfStackIsEmpty(state);

                        state.Stack[state.StackHead - 1] = state.Stack[state.StackHead - 1] != 0 ? FalseValue : TrueValue;
                        state.CurrentInstructionIndex += 1;

                        break;   
                    }
                    case Instruction.OpAdd:
                    case Instruction.OpSubtract:
                    case Instruction.OpMultiply:
                    case Instruction.OpDivide:
                    case Instruction.OpMod:
                    case Instruction.OpLessThan:
                    case Instruction.OpLessThanEqual:
                    case Instruction.OpGreaterThan:
                    case Instruction.OpGreaterThanEqual:
                    case Instruction.OpEqual:
                    case Instruction.OpNotEqual:
                    case Instruction.OpAnd:
                    case Instruction.OpOr:
                    {
                        ThrowIfStackDoesNotContainEnoughValues(state, 2);

                        var val1 = state.Stack[state.StackHead - 2];
                        var val2 = state.Stack[state.StackHead - 1];
                        var newValue = val1;

                        switch (inst) {
                            case Instruction.OpAdd:                 newValue = val1 + val2; break;
                            case Instruction.OpSubtract:            newValue = val1 - val2; break;
                            case Instruction.OpMultiply:            newValue = val1 * val2; break;
                            case Instruction.OpDivide: 
                                if(val2 == 0.0f) throw new InvalidOperationException("Cannot divide by zero.");
                                newValue = val1 / val2; break;
                            case Instruction.OpMod:                 newValue = val1 %  val2; break;
                            case Instruction.OpLessThan:            newValue = val1 <  val2 ? TrueValue : FalseValue; break;
                            case Instruction.OpLessThanEqual:       newValue = val1 <= val2 ? TrueValue : FalseValue; break;
                            case Instruction.OpGreaterThan:         newValue = val1 >  val2 ? TrueValue : FalseValue; break;
                            case Instruction.OpGreaterThanEqual:    newValue = val1 >= val2 ? TrueValue : FalseValue; break;
                            case Instruction.OpEqual:               newValue = val1 == val2 ? TrueValue : FalseValue; break;
                            case Instruction.OpNotEqual:            newValue = val1 != val2 ? TrueValue : FalseValue; break;
                            case Instruction.OpAnd:                 newValue = val1 != 0 && val2 != 0 ? TrueValue : FalseValue; break;
                            case Instruction.OpOr:                  newValue = val1 != 0 || val2 != 0 ? TrueValue : FalseValue; break;
                        }

                        state.Stack[state.StackHead - 2] = newValue;
                        state.StackHead--;
                        state.CurrentInstructionIndex += 1;

                        break;
                    }
                    case Instruction.Jump:
                    {
                        var address = (int) code[state.CurrentInstructionIndex + 1];

                        if(address < 0 || address >= code.Size) throw new IndexOutOfRangeException(String.Format("Jump address is out-of-range (jump: {0}, size: {1}).", address, code.Size));

                        state.CurrentInstructionIndex = address;

                        break;
                    }
                    case Instruction.JumpIfZero:
                    {
                        var address = (int) code[state.CurrentInstructionIndex + 1];

                        if (address < 0 || address >= code.Size) throw new IndexOutOfRangeException(String.Format("Jump address is out-of-range (jump: {0}, size: {1}).", address, code.Size));

                        if (state.Stack[state.StackHead - 1] == 0) {
                            state.CurrentInstructionIndex = address;
                        }
                        else {
                            state.CurrentInstructionIndex += 2;
                        }

                        break;
                    }
                    case Instruction.Compare:
                        break;
                    case Instruction.Return:
                        break;
                    case Instruction.FunctionCall:
                        break;
                    case Instruction.GetUpdater:
                        break;
                    case Instruction.GetProperty:
                        break;
                    case Instruction.SetProperty:
                        break;
                    case Instruction.GetVariable:
                        break;
                    case Instruction.SetVariable:
                        break;
                    case Instruction.Fire:
                        break;
                    case Instruction.WaitFrames:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (state.CurrentInstructionIndex >= code.Size) {
                    if (looping) {
                        state.CurrentInstructionIndex = 0;
                    }
                    else {
                        return ScriptStatus.Ok;
                    }
                }
            }
        }

        private void ThrowIfStackLimitIsReached(ScriptState state)
        {
            if (state.StackHead >= MaxStackSize) {
                throw new InvalidOperationException("Stack limit reached!");
            }
        }

        private void ThrowIfStackBaseIsHit(ScriptState state)
        {
            if (state.StackHead < 0) {
                throw new InvalidOperationException("Stack base has been hit!");
            }
        }

        private void ThrowIfStackIsEmpty(ScriptState state)
        {
            if (state.StackHead == 0) {
                throw new InvalidOperationException("There must be a value on the stack in order to execute this instruction.");
            }
        }

        private void ThrowIfStackDoesNotContainEnoughValues(ScriptState state, int minimumNumberOfValues)
        {
            if (minimumNumberOfValues == 1) {
                ThrowIfStackIsEmpty(state);
            }
            else if (state.StackHead == 0) {
                throw new InvalidOperationException("There must be at least " + minimumNumberOfValues + " values on the stack in order to execute this instruction.");
            }
        }
    }
}
