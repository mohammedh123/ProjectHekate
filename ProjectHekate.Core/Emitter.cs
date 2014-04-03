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

        void Update(float dt);
    }

    public class Emitter : IEmitter
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public bool Enabled { get; set; }

        public void Update(float dt)
        {
            throw new NotImplementedException();
        }
    }
}
