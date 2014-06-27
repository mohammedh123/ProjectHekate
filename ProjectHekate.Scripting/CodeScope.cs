using System;
using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class CodeScope : CodeBlock, ISymbolContext
    {
        private readonly IList<ISymbol> _symbols;
        private readonly IDictionary<string, int> _symbolsNameToIndex;

        public CodeScope()
        {
            _symbols = new List<ISymbol>();
            _symbolsNameToIndex = new Dictionary<string, int>();
        }

        public int AddSymbol(string name, SymbolType symbolType)
        {
            if (_symbolsNameToIndex.ContainsKey(name))
                throw new ArgumentException("A variable with the name \"" + name + "\" already exists in this scope.", "name");

            var newIdx = _symbols.Count;
            ISymbol newSymbol;

            switch (symbolType) {
                case SymbolType.Numerical: newSymbol = new NumericalSymbol(name, newIdx);  break;
                case SymbolType.Emitter:   newSymbol = new EmitterSymbol(name, newIdx);    break;
                default:
                    throw new ArgumentOutOfRangeException("symbolType", "An unknown symbol type was found when attempting to add a new symbol.");
            }

            _symbols.Add(newSymbol);
            _symbolsNameToIndex[name] = newIdx;

            return newIdx;
        }

        public ISymbol GetSymbol(string name)
        {
            int index;
            var worked = _symbolsNameToIndex.TryGetValue(name, out index);

            if (!worked) throw new ArgumentException("A symbol with the name " + name + " could not be found.");

            return _symbols[index];
        }
    }
}