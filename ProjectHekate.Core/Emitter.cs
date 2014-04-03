using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public delegate IEnumerator<WaitInFrames> EmitterUpdateDelegate(Emitter emitter);

    public interface IEmitter
    {
        float X { get; set; }
        float Y { get; set; }
        float Angle { get; set; }
        bool Enabled { get; set; }
    }

    public class Emitter : IEmitter
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public bool Enabled { get; set; }

        internal Emitter()
        {}

        internal IEnumerator<WaitInFrames> Update()
        {
            return UpdateFunc != null ? UpdateFunc(this) : null;
        }

        internal EmitterUpdateDelegate UpdateFunc { get; set; }
    }
}
