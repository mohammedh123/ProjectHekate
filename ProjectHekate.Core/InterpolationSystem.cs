using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Math = ProjectHekate.Core.Helpers.Math;

namespace ProjectHekate.Core
{
    public interface IInterpolationSystem
    {
        void InterpolateEmitterAngle(IEmitter e, float initialAngle, float finalAngle, int inHowManyFrames);
        void InterpolateProjectileAngle(AbstractProjectile proj, float initialAngle, float finalAngle, int inHowManyFrames);
    }

    internal class InterpolationSystem : IInterpolationSystem
    {
        private interface IProjectileInterpolator
        {
            int CurrentFrame { get; set; }
            bool IsFinished { get; }

            void ApplyInterpolation();
        }

        private class ProjectileAngleInterpolator : IProjectileInterpolator
        {
            private readonly AbstractProjectile _projectile;
            private readonly float _initialAngle;
            private readonly float _finalAngle;
            private readonly int _frameCount;

            public int CurrentFrame { get; set; }

            public bool IsFinished => CurrentFrame == _frameCount+1;

            public ProjectileAngleInterpolator(AbstractProjectile proj, float initialAngle, float finalAngle, int frameCount)
            {
                _projectile = proj;
                _initialAngle = initialAngle;
                _finalAngle = finalAngle;
                _frameCount = frameCount;
                CurrentFrame = 0;
            }

            public void ApplyInterpolation()
            {
                _projectile.Angle = Math.Lerp(_initialAngle, _finalAngle, CurrentFrame/(float)_frameCount);
            }
        }

        private readonly List<IProjectileInterpolator> _projectileInterpolators;

        public InterpolationSystem()
        {
            _projectileInterpolators = new List<IProjectileInterpolator>(10000);    
        }

        public void InterpolateEmitterAngle(IEmitter e, float initialAngle, float finalAngle, int inHowManyFrames)
        {
            throw new NotImplementedException();
        }

        public void InterpolateProjectileAngle(AbstractProjectile proj, float initialAngle, float finalAngle, int inHowManyFrames)
        {
            _projectileInterpolators.Add(new ProjectileAngleInterpolator(proj, initialAngle, finalAngle, inHowManyFrames));
        }

        internal void Update()
        {
            for (int i = 0; i < _projectileInterpolators.Count; i++) {
                var bulletInterpolator = _projectileInterpolators[i];

                bulletInterpolator.CurrentFrame++;
                bulletInterpolator.ApplyInterpolation();

                if (bulletInterpolator.IsFinished) {
                    _projectileInterpolators.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
