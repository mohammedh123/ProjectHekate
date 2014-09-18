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
        public IList<float> Stack { get; set; }
        public int StackHead { get; set; }

        internal float[] NumericalVariables;
        internal object[] EmitterVariables; // TODO: change this to SOMETHING else


        public ScriptState()
        {
            NumericalVariables = new float[VirtualMachine.MaxNumericalVariables];
            EmitterVariables = new object[VirtualMachine.MaxEmitterVariables];

            Stack = new float[VirtualMachine.MaxStackSize];
            StackHead = 0;
            SuspendTime = 0;
        }
    }
}