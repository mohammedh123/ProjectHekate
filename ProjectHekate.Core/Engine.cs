using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public class AbstractScriptedObject<TUpdateFunc> where TUpdateFunc : class
    {
        internal float WaitTimer { get; set; }
        internal IEnumerator<WaitInFrames> WaitEnumerator { get; set; }
        public bool IsScripted { get { return UpdateFunc == null; } }

        internal TUpdateFunc UpdateFunc { get; set; }
    }

    public interface IEngine
    {
        IBulletSystem BulletSystem { get; }
        IInterpolationSystem InterpolationSystem { get; }

        /// <summary>
        /// Creates a controller that (should be) controlled by an external entity. The controller returned
        /// by calling .Build() should have its properties set to the controlling entity's properties.
        /// </summary>
        ControllerBuilder CreateController(float x, float y, float angle, bool enabled);
        /// <summary>
        /// Creates a controller that is controlled via a script function. When associating the controller 
        /// returned by calling .Build() with an entity in your game, instead of setting its properties from
        /// the entity's properties, the entity should have its properties set from the controller's 
        /// properties.
        /// </summary>
        ControllerBuilder CreateScriptedController(float x, float y, float angle, bool enabled,  ControllerUpdateDelegate updateFunc);
        void Update(float dt);
    }
    
    public class Engine : IEngine
    {
        private readonly BulletSystem _bulletSystem;
        private readonly InterpolationSystem _interpolationSystem;
        private readonly List<Controller> _controllers;

        private readonly List<Emitter> _emitters;

        public IBulletSystem BulletSystem
        {
            get { return _bulletSystem; }
        }

        public IInterpolationSystem InterpolationSystem
        {
            get { return _interpolationSystem; }
        }

        public Engine()
        {
            _bulletSystem = new BulletSystem();
            _interpolationSystem = new InterpolationSystem();
            _controllers = new List<Controller>();
            _emitters = new List<Emitter>();
        }

        public ControllerBuilder CreateController(float x, float y, float angle, bool enabled)
        {
            return new ControllerBuilder(x, y, angle, enabled, this, null);
        }

        public ControllerBuilder CreateScriptedController(float x, float y, float angle, bool enabled, ControllerUpdateDelegate updateFunc)
        {
            return new ControllerBuilder(x, y, angle, enabled, this, updateFunc);
        }

        internal void AddController(Controller con)
        {
            _controllers.Add(con);
        }

        internal void AddEmitters(IEnumerable<Emitter> e)
        {
            _emitters.AddRange(e);
        }
        
        public void Update(float dt)
        {
            // updates all systems
            UpdateControllers();

            _bulletSystem.Update(dt, _interpolationSystem);
            _interpolationSystem.Update();
        }

        private void UpdateControllers()
        {
            foreach (var controller in _controllers) {
                if (controller.IsEnabled) {
                    UpdateControllersEmitters(controller);
                }

                // if the controller does not have a special update function, skip the wait logic
                if(controller.UpdateFunc == null)
                {
                    controller.FramesAlive++;
                    continue;
                }

                // if the controller's "update state" is ready
                if (controller.WaitTimer <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        controller.WaitEnumerator = controller.WaitEnumerator ?? controller.Update(this);

                        // this steps through the controllers update function until it hits a yield return
                        if (controller.WaitEnumerator.MoveNext())
                        {
                            // TODO: check the type of controller.WaitEnumerator.Current to make sure it isn't null?
                            // starting next frame, this controller is 'waiting'
                            controller.WaitTimer = controller.WaitEnumerator.Current.Delay;

                            loopAgain = false;
                        }
                        else
                        { // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            controller.WaitEnumerator = controller.Update(this);

                            loopAgain = true;
                        }
                    }
                }
                else
                { // the controller is "waiting"
                    controller.WaitTimer--;
                }
            }
        }

        private void UpdateControllersEmitters(Controller cont)
        {
            foreach(var emitter in cont.Emitters)
            {
                if (!emitter.Enabled)
                {
                    continue;
                }

                while (emitter.Angle > Helpers.Math.TwoPi) emitter.Angle -= Helpers.Math.TwoPi;
                while (emitter.Angle < -Helpers.Math.TwoPi) emitter.Angle += Helpers.Math.TwoPi;

                if (emitter.Orbiting) {
                    // use angle + distance to determine position
                    emitter.X = cont.X + (float) Math.Cos(emitter.Angle)*emitter.OrbitDistance;
                    emitter.Y = cont.Y + (float) Math.Sin(emitter.Angle)*emitter.OrbitDistance;
                }
                else {
                    emitter.X = cont.X + emitter.OffsetX;
                    emitter.Y = cont.Y + emitter.OffsetY;
                }

                // if the emitter does not have a special update function, skip the wait logic
                if (emitter.UpdateFunc == null) {
                    emitter.FramesAlive++;
                    continue;
                }

                // if the emitter's "update state" is ready
                if (emitter.WaitTimer <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        emitter.WaitEnumerator = emitter.WaitEnumerator ?? emitter.Update(this);

                        // this steps through the emitters update function until it hits a yield return
                        if (emitter.WaitEnumerator.MoveNext())
                        {
                            // TODO: check the type of emitter.WaitEnumerator.Current to make sure it isn't null?
                            // starting next frame, this emitter is 'waiting'
                            emitter.WaitTimer = emitter.WaitEnumerator.Current.Delay;

                            loopAgain = false;
                        }
                        else
                        { // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            emitter.WaitEnumerator = emitter.Update(this);

                            loopAgain = true;
                        }
                    }
                }
                else
                { // the emitter is "waiting"
                    emitter.WaitTimer--;
                }

                emitter.FramesAlive++;
            }
        }
    }
}
