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
        void InterpolateBulletAngle(Bullet bullet, float initialAngle, float finalAngle, int inHowManyFrames);
    }

    internal class InterpolationSystem : IInterpolationSystem
    {
        private interface IBulletInterpolator
        {
            int CurrentFrame { get; set; }
            bool IsFinished { get; }

            void ApplyInterpolation();
        }

        private class BulletAngleInterpolator : IBulletInterpolator
        {
            private readonly Bullet _bullet;
            private readonly float _initialAngle;
            private readonly float _finalAngle;
            private readonly int _frameCount;

            public int CurrentFrame { get; set; }

            public bool IsFinished {
                get { return CurrentFrame == _frameCount+1; }
            }

            public BulletAngleInterpolator(Bullet bullet, float initialAngle, float finalAngle, int frameCount)
            {
                _bullet = bullet;
                _initialAngle = initialAngle;
                _finalAngle = finalAngle;
                _frameCount = frameCount;
                CurrentFrame = 0;
            }

            public void ApplyInterpolation()
            {
                _bullet.Angle = Math.Lerp(_initialAngle, _finalAngle, CurrentFrame/(float)_frameCount);
            }
        }

        private readonly List<IBulletInterpolator> _bulletInterpolators;

        public InterpolationSystem()
        {
            _bulletInterpolators = new List<IBulletInterpolator>(10000);    
        }

        public void InterpolateEmitterAngle(IEmitter e, float initialAngle, float finalAngle, int inHowManyFrames)
        {
            throw new NotImplementedException();
        }

        public void InterpolateBulletAngle(Bullet bullet, float initialAngle, float finalAngle, int inHowManyFrames)
        {
            _bulletInterpolators.Add(new BulletAngleInterpolator(bullet, initialAngle, finalAngle, inHowManyFrames));
        }

        internal void Update()
        {
            for (int i = 0; i < _bulletInterpolators.Count; i++) {
                var bulletInterpolator = _bulletInterpolators[i];

                bulletInterpolator.CurrentFrame++;
                bulletInterpolator.ApplyInterpolation();

                if (bulletInterpolator.IsFinished) {
                    _bulletInterpolators.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
