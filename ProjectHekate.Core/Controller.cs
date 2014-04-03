using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    /// <summary>
    /// Controllers are objects that have emitters. The controller is disabled by default.
    /// </summary>
    public interface IController
    {
        float X { get; set; }
        float Y { get; set; }
        float Angle { get; set; }
        bool Enabled { get; set; }

        void AddEmitter(IEmitter e);
    }

    class Controller : IController
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public bool Enabled { get; set; }

        private readonly List<IEmitter> _emitters = new List<IEmitter>();


        internal Controller()
        {}

        public void AddEmitter(IEmitter e)
        {
            _emitters.Add(e);
        }
    }
}
