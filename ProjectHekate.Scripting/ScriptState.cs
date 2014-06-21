using System;
using System.Collections.Generic;

namespace ProjectHekate.Scripting
{
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
}