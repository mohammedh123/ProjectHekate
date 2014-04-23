using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public class EmitterBuilder
    {
        private readonly Emitter _emitter;
        private readonly Engine _engine;

        // normal emitter
        internal EmitterBuilder(float x, float y, float angle, bool enabled, EmitterUpdateDelegate updateFunc, Engine e)
        {
            _emitter = new Emitter()
            {
                X = x,
                Y = y,
                Angle = angle,
                IsEnabled = enabled,
                UpdateFunc = updateFunc
            };

            _engine = e;
        }

        public EmitterBuilder WithEmitter(float x, float y, float angle, bool enabled, EmitterUpdateDelegate updater)
        {
            var emitter = new Emitter { OffsetX = x, OffsetY = y, Angle = angle, IsEnabled = enabled, UpdateFunc = updater };
            _emitter.Emitters.Add(emitter);

            return this;
        }

        public EmitterBuilder WithOrbittingEmitter(float distance, float angle, bool enabled,
            EmitterUpdateDelegate updater)
        {
            var emitter = new Emitter { OrbitDistance = distance, Angle = angle, IsEnabled = enabled, UpdateFunc = updater, Orbiting = true };
            _emitter.Emitters.Add(emitter);

            return this;
        }

        public IEmitter Build()
        {
            _engine.AddEmitter(_emitter);
            return _emitter;
        }
    }
}
