using System.Collections.Generic;

namespace ProjectHekate.Scripting
{
    class EmitterDefinition
    {
        public string Name { get; set; }

        private Dictionary<string, int> _intVariables;
        private Dictionary<string, float> _floatVariables;

        public EmitterDefinition()
        {
            _intVariables = new Dictionary<string, int>();
            _floatVariables = new Dictionary<string, float>();
        }

        public void AddLocalInt(string name, int value = 0)
        {
            
        }
    }
}
