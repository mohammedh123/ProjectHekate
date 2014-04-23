using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Core.Interfaces;

namespace ProjectHekate.Core
{
    public delegate IEnumerator<WaitInFrames> ControllerUpdateDelegate(IController c, IEngine engine);

    /// <summary>
    /// Controllers are objects that have emitters. The controller is disabled by default.
    /// </summary>
    public interface IController : IPositionable
    {
        int FramesAlive { get; }
        bool IsEnabled { get; set; }
    }

    class Controller : AbstractScriptedObject<ControllerUpdateDelegate>, IController
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public bool IsEnabled { get; set; }
        public int FramesAlive { get; set; }

        internal IEnumerator<WaitInFrames> Update(IEngine engine)
        {
            return UpdateFunc != null ? UpdateFunc(this, engine) : null;
        }

        internal readonly List<Emitter> Emitters = new List<Emitter>();


        internal Controller()
        {}
    }
}
