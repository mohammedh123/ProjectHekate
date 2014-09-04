using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ProjectHekate.Scripting.Tests")]
namespace ProjectHekate.Scripting
{
    public enum ScriptStatus
    {
        Ok,
        Error,
        Suspended
    }

    public class ScriptState
    {
        public int CurrentInstructionIndex { get; set; }
        public int CodeBlockIndex { get; set; }
        public int SuspendTime { get; set; }

        internal float[] NumericalVariables;
        internal object[] EmitterVariables; // TODO: change this to SOMETHING else
        internal float[] Properties;

        internal IList<float> Stack;
        internal int StackHead;

        public ScriptState()
        {
            NumericalVariables = new float[VirtualMachine.MaxNumericalVariables];
            EmitterVariables = new object[VirtualMachine.MaxEmitterVariables];
            Properties = new float[VirtualMachine.MaxProperties];

            Stack = new float[VirtualMachine.MaxStackSize];
            StackHead = 0;
            SuspendTime = 0;
        }
    }
}