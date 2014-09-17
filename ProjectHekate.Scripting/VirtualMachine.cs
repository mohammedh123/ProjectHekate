using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime;
using ProjectHekate.Grammar;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class FunctionDefinition
    {
        public string Name { get; set; }
        
        public int Index { get; set; }

        public Func<ScriptState, ScriptStatus> Function { get; set; } 
    }

    public class VirtualMachine : IVirtualMachine, IBytecodeInterpreter
    {
        private const float TrueValue = 1.0f;
        private const float FalseValue = 0.0f;

        public const int MaxNumericalVariables = 64;
        public const int MaxEmitterVariables = 8;
        public const int MaxProperties = 32;
        public const int MaxStackSize = 64;
        
        private readonly List<FunctionCodeScope> _functionCodeScopes;
        private readonly List<ActionCodeScope> _actionCodeScopes;
        private readonly List<EmitterUpdaterCodeScope> _emitterUpdaterCodeScopes;
        private readonly Dictionary<string, int> _functionCodeScopeNameToIndex;
        private readonly Dictionary<string, int> _actionCodeScopeNameToIndex;
        private readonly Dictionary<string, int> _emitterUpdaterCodeScopeNameToIndex;

        private readonly List<ITypeDefinition> _typeDefinitions;
        private readonly Dictionary<string, int> _typeNameToIndex;
        
        private readonly IDictionary<string, float> _globalSymbolsNameToValue;

        private readonly List<string> _globalPropertyList;

        private readonly List<FunctionDefinition> _externalFunctions;
        private readonly Dictionary<string, int> _externalFunctionNameToIndex;

        public VirtualMachine()
        {
            _functionCodeScopes = new List<FunctionCodeScope>();
            _functionCodeScopeNameToIndex = new Dictionary<string, int>();

            _actionCodeScopes = new List<ActionCodeScope>();
            _actionCodeScopeNameToIndex = new Dictionary<string, int>();

            _emitterUpdaterCodeScopes = new List<EmitterUpdaterCodeScope>();
            _emitterUpdaterCodeScopeNameToIndex = new Dictionary<string, int>();

            _typeDefinitions = new List<ITypeDefinition>();
            _typeNameToIndex = new Dictionary<string, int>();

            _globalSymbolsNameToValue = new Dictionary<string, float>();

            _globalPropertyList = new List<string>();

            _externalFunctions = new List<FunctionDefinition>();
            _externalFunctionNameToIndex = new Dictionary<string, int>();
        }

        public int AddFunctionCodeScope(string name, FunctionCodeScope codeScope)
        {
            return AddSpecializedCodeScope(name, _functionCodeScopes, _functionCodeScopeNameToIndex, codeScope);
        }

        public FunctionCodeScope GetFunctionCodeScope(string name)
        {
            return GetSpecializedCodeScope(name, "function", _functionCodeScopes, _functionCodeScopeNameToIndex);
        }

        public int AddActionCodeScope(string name, ActionCodeScope codeScope)
        {
            return AddSpecializedCodeScope(name, _actionCodeScopes, _actionCodeScopeNameToIndex, codeScope);
        }

        public ActionCodeScope GetActionCodeScope(string name)
        {
            return GetSpecializedCodeScope(name, "action", _actionCodeScopes, _actionCodeScopeNameToIndex);
        }

        public int AddEmitterUpdaterCodeScope(string name, EmitterUpdaterCodeScope codeScope)
        {
            return AddSpecializedCodeScope(name, _emitterUpdaterCodeScopes, _emitterUpdaterCodeScopeNameToIndex, codeScope);
        }

        public EmitterUpdaterCodeScope GetEmitterUpdaterCodeScope(string name)
        {
            return GetSpecializedCodeScope(name, "emitter updater", _emitterUpdaterCodeScopes, _emitterUpdaterCodeScopeNameToIndex);
        }

        public void AddProperty<TScriptObjectType>(string typeName, params Expression<Func<TScriptObjectType, float>>[] propertyExpressions) where TScriptObjectType : AbstractScriptObject
        {
            foreach (var expression in propertyExpressions) {
                var type = GetType(typeName);
                var idx = type.AddProperty(expression);
                var prop = type.GetProperty(idx);

                var exists = _globalPropertyList.Contains(prop.Name);
                if (!exists) _globalPropertyList.Add(prop.Name);
            }
        }

        public int GetPropertyIndex(string propertyName)
        {
            return _globalPropertyList.IndexOf(propertyName);
        }

        public void LoadCode(string text)
        {
            var lexer = new HekateLexer(new AntlrInputStream(text));
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);

            var scriptContext = parser.script();

            var visitor = new HekateScriptVisitor();
            var emitter = visitor.Visit(scriptContext);

            emitter.EmitTo(null, this, new StackScopeManager());

            UpdatePropertyMappings();
        }

        internal void UpdatePropertyMappings()
        {
            // now loop through all the emit types and map the properties correctly
            foreach (var emitType in _typeDefinitions)
            {
                emitType.UpdatePropertyMappings(_globalPropertyList);
            }
        }

        public void Update<TScriptObjectType>(TScriptObjectType so) where TScriptObjectType : AbstractScriptObject
        {
            if (so.ScriptState.SuspendTime <= 0) {
                InterpretCode(_actionCodeScopes[so.ScriptState.CodeBlockIndex], so.ScriptState, so, true);
            }
            else {
                so.ScriptState.SuspendTime--;
            }
        }

        public void AddExternalFunction(string functionName, Func<ScriptState, ScriptStatus> function)
        {
            if (_externalFunctionNameToIndex.ContainsKey(functionName))
                throw new ArgumentException("An external function with the name \"" + functionName+ "\" has already been registered.", "functionName");

            var functionDef = new FunctionDefinition()
            {
                Function = function,
                Name = functionName,
                Index = _externalFunctions.Count
            };

            _externalFunctions.Add(functionDef);
            _externalFunctionNameToIndex[functionName] = functionDef.Index;
        }

        public FunctionDefinition GetExternalFunction(string functionName)
        {
            if (_externalFunctionNameToIndex.ContainsKey(functionName)) {
                return _externalFunctions[_externalFunctionNameToIndex[functionName]];
            }

            return null;
        }

        public FunctionDefinition GetExternalFunctionByIndex(int idx)
        {
            return _externalFunctions[idx];
        }

        public int AddType<TScriptObjectType>(string typeName) where TScriptObjectType : AbstractScriptObject
        {
            if (_typeNameToIndex.ContainsKey(typeName))
                throw new ArgumentException("A type with the name \"" + typeName + "\" already exists in this machine.", "typeName");

            var typeDefinition = new TypeDefinition(typeName, _typeDefinitions.Count);
            _typeDefinitions.Add(typeDefinition);
            _typeNameToIndex[typeName] = typeDefinition.Index;

            return typeDefinition.Index;
        }

        public ITypeDefinition GetType(string typeName)
        {
            if (!_typeNameToIndex.ContainsKey(typeName))
                throw new ArgumentException("A type with the name \"" + typeName + "\" could not be found.", typeName);

            return _typeDefinitions[_typeNameToIndex[typeName]];
        }

        private int AddSpecializedCodeScope<TCodeScopeType>(string name, ICollection<TCodeScopeType> codeScopeList, IDictionary<string,int> codeScopeNameToIndexMap, TCodeScopeType codeScope) where TCodeScopeType : CodeScope
        {
            // TODO: make method thread-safe
            ThrowIfCodeScopeWithNameAlreadyExists(name);
            ThrowIfCodeScopeAlreadyExists(codeScope);

            codeScopeList.Add(codeScope);
            codeScope.Index = codeScopeList.Count - 1;
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
            if (_actionCodeScopeNameToIndex.ContainsKey(name))
                throw new ArgumentException("An action with the name \"" + name + "\" already exists in this script.", "name");
            if (_emitterUpdaterCodeScopeNameToIndex.ContainsKey(name))
                throw new ArgumentException("An emitter updater with the name \"" + name + "\" already exists in this script.", "name");
        }

        private void ThrowIfCodeScopeAlreadyExists(CodeScope codeScope)
        {
            if (_functionCodeScopes.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as a function).", "codeScope");
            if (_actionCodeScopes.Contains(codeScope))
                throw new ArgumentException("This code scope has already been added, but with a different name (as an action).", "codeScope");
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

        public ScriptStatus InterpretCode(ICodeBlock code, ScriptState state, AbstractScriptObject obj, bool looping)
        {
            if (code == null) throw new ArgumentNullException("code");
            if (state == null) throw new ArgumentNullException("state");
            if (state.CurrentInstructionIndex >= code.Size) return ScriptStatus.Ok;


            while (true) {
                var beginningInstructionIndex = state.CurrentInstructionIndex;
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
                    case Instruction.GetProperty:
                    {
                        var idx = (int)code[state.CurrentInstructionIndex + 1];
                        var type = _typeDefinitions[obj.EmitTypeIndex];
                        var prop = type.GetPropertyByGlobalIndex(idx);
                        var val = prop.GetValue(obj);

                        state.Stack[state.StackHead] = val;
                        state.StackHead++;
                        state.CurrentInstructionIndex += 2;

                        ThrowIfStackLimitIsReached(state);

                        break;
                    }
                    case Instruction.SetProperty:
                    {
                        ThrowIfStackIsEmpty(state);

                        var val = state.Stack[state.StackHead - 1];
                        var idx = (int)code[state.CurrentInstructionIndex + 1];
                        var type = _typeDefinitions[obj.EmitTypeIndex];
                        var prop = type.GetPropertyByGlobalIndex(idx);

                        state.CurrentInstructionIndex += 2;
                        prop.SetValue(obj, val);

                        break;
                    }
                    case Instruction.GetVariable:
                    {
                        var idx = (int)code[state.CurrentInstructionIndex + 1];
                        var val = state.NumericalVariables[idx];

                        state.Stack[state.StackHead] = val;
                        state.StackHead++;
                        state.CurrentInstructionIndex += 2;

                        ThrowIfStackLimitIsReached(state);

                        break;
                    }
                    case Instruction.SetVariable:
                    {
                        ThrowIfStackIsEmpty(state);

                        var val = state.Stack[state.StackHead - 1];
                        var idx = (int)code[state.CurrentInstructionIndex + 1];
                        state.NumericalVariables[idx] = val;

                        state.CurrentInstructionIndex += 2;

                        break;
                    }
                    case Instruction.WaitFrames:
                    {
                        ThrowIfStackIsEmpty(state);

                        var val = (int)state.Stack[--state.StackHead];

                        state.SuspendTime = val;
                        state.CurrentInstructionIndex += 1;

                        if (state.CurrentInstructionIndex >= code.Size && looping) { // need to do the check here because we're returning early
                            state.CurrentInstructionIndex = 0;
                        }
                        
                        ThrowIfStackBaseIsHit(state);

                        return ScriptStatus.Suspended;
                    }
                    case Instruction.ExternalFunctionCall:
                    {
                        var idx = (int)code[state.CurrentInstructionIndex + 1];
                        var externalFunction = GetExternalFunctionByIndex(idx);

                        var returnStatus = externalFunction.Function(state);
                        state.CurrentInstructionIndex += 2;

                        if (returnStatus == ScriptStatus.Suspended)
                        { // need to do the check here because we're returning early
                            if (state.CurrentInstructionIndex >= code.Size && looping) {
                                state.CurrentInstructionIndex = 0;
                            }

                            return returnStatus;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(String.Format("One of the instruction types, {0}, was not implemented!", inst), (Exception)null);
                }

                if (state.CurrentInstructionIndex == beginningInstructionIndex) {
                    throw new InvalidOperationException(String.Format("The current instruction index did not advance this tick; check InterpretCode and make sure {0} is implemented properly.", inst));
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
