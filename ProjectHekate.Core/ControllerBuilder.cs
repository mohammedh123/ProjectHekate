﻿using System.Collections.Generic;

namespace ProjectHekate.Core
{
    public class ControllerBuilder
    {
        private readonly Controller _controller;
        private readonly List<Emitter> _emitters; 
        private readonly Engine _engine;

        internal static EmitterUpdateWithInterpolationDelegate ConvertUpdateDelegateToInterpolationDelegate(EmitterUpdateDelegate updater)
        {
            return (e, bs, ins) => updater(e, bs);
        }

        internal ControllerBuilder(float x, float y, float angle, bool enabled, Engine e)
        {
            _controller = new Controller()
                          {
                              X = x,
                              Y = y,
                              Angle = angle,
                              Enabled = enabled
                          };
            _emitters = new List<Emitter>();
            _engine = e;
        }

        public ControllerBuilder WithEmitter(float x, float y, float angle, bool enabled, EmitterUpdateDelegate updater)
        {
            return WithEmitter(x, y, angle, enabled, ConvertUpdateDelegateToInterpolationDelegate(updater));
        }

        public ControllerBuilder WithEmitter(float x, float y, float angle, bool enabled, EmitterUpdateWithInterpolationDelegate updater)
        {
            var emitter = new Emitter {OffsetX = x, OffsetY = y, Angle = angle, Enabled = enabled, UpdateFunc = updater};
            _controller.Emitters.Add(emitter);
            _emitters.Add(emitter);

            return this;
        }

        public ControllerBuilder WithOrbittingEmitter(float distance, float angle, bool enabled, EmitterUpdateDelegate updater)
        {
            return WithOrbittingEmitter(distance, angle, enabled, ConvertUpdateDelegateToInterpolationDelegate(updater));
        }

        public ControllerBuilder WithOrbittingEmitter(float distance, float angle, bool enabled,
            EmitterUpdateWithInterpolationDelegate updater)
        {
            var emitter = new Emitter {OrbitDistance = distance, Angle = angle, Enabled = enabled, UpdateFunc = updater, Orbiting = true};
            _controller.Emitters.Add(emitter);
            _emitters.Add(emitter);

            return this;
        }

        public IController Build()
        {
            _engine.AddController(_controller);
            _engine.AddEmitters(_emitters);
            return _controller;
        }
    }
}