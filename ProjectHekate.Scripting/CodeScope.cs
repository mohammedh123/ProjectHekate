using System;
using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class CodeScope : CodeBlock, IVariableContext
    {
        private readonly List<IdentifierRecord> _numericalVariables;
        private readonly Dictionary<string, int> _numericalVariablesNameToIndex;

        private readonly List<IdentifierRecord> _emitterVariables;
        private readonly Dictionary<string, int> _emitterVariablesNameToIndex;
        
        public CodeScope()
        {
            _numericalVariables = new List<IdentifierRecord>();
            _numericalVariablesNameToIndex = new Dictionary<string, int>();

            _emitterVariables = new List<IdentifierRecord>();
            _emitterVariablesNameToIndex = new Dictionary<string, int>();
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
}