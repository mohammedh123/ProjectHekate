using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public delegate IEnumerator<WaitInFrames> EmitterUpdateDelegate(Emitter emitter, IBulletSystem bs);

    /// <summary>
    /// Emitters are objects that fire off bullets. They are attached to a controller (you should not have a dangling emitter) and their positions are offset from the controller's position.
    /// </summary>
    public interface IEmitter
    {
        float X { get; }
        float Y { get; }
        float OffsetX { get; set; }
        float OffsetY { get; set; }
        float Angle { get; set; }
        bool Enabled { get; set; }
    }

    public class Emitter : IEmitter
    {
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float Angle { get; set; }
        public bool Enabled { get; set; }

        public float X { get; internal set; }
        public float Y { get; internal set; }
        internal float WaitTimer { get; set; }
        internal IEnumerator<WaitInFrames> WaitEnumerator { get; set; } 

        internal Emitter()
        {}

        internal IEnumerator<WaitInFrames> Update(IBulletSystem bs)
        {
            return UpdateFunc != null ? UpdateFunc(this, bs) : null;
        }

        internal EmitterUpdateDelegate UpdateFunc { get; set; }
    }
}
