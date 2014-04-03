using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public interface IEngine
    {
        IController CreateController(float x, float y, float angle, bool enabled);
        IEmitter CreateEmitter(float x, float y, float angle, bool enabled, EmitterUpdateDelegate updater);
    }

    public class Engine : IEngine
    {
        public IController CreateController(float x, float y, float angle, bool enabled)
        {
            throw new NotImplementedException();
        }

        public IEmitter CreateEmitter(float x, float y, float angle, bool enabled, EmitterUpdateDelegate updater)
        {
            throw new NotImplementedException();
        }
    }
}
