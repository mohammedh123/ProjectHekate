using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public interface IBulletSystem
    {
        IBullet FireBasicBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex);
        IBullet FireScriptedBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex, BulletUpdateDelegate bulletFunc);

        IReadOnlyCollection<IBullet> Bullets { get; }

        void Update(float dt);
    }

    public class BulletSystem : IBulletSystem
    {
        public const int MaxBullets = 2048;

        // TODO: break bullets into arrays of components
        private readonly Bullet[] _bullets = new Bullet[MaxBullets];
        private readonly float[] _bulletWaitTimers = new float[MaxBullets];
        private readonly IEnumerator<WaitInFrames>[] _bulletEnumerators = new IEnumerator<WaitInFrames>[MaxBullets];
        private int _availableBulletIndex;

        public IReadOnlyCollection<IBullet> Bullets { get; private set; }

        public BulletSystem()
        {
            for (var i = 0; i < MaxBullets; i++) {
                _bullets[i] = new Bullet();
                _bulletWaitTimers[i] = -1.0f;
                _bulletEnumerators[i] = null;
            }

            Bullets = Array.AsReadOnly(_bullets);
        }

        public IBullet FireBasicBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex)
        {
            return InternalFireBullet(x, y, angle, speedPerFrame, spriteIndex, null);
        }

        public IBullet FireScriptedBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex, BulletUpdateDelegate bulletFunc)
        {
            return InternalFireBullet(x, y, angle, speedPerFrame, spriteIndex, bulletFunc);
        }

        private Bullet InternalFireBullet(float x, float y, float angle, float speed, int spriteIndex, BulletUpdateDelegate bulletFunc)
        {
            var bullet = FindNextAvailableBullet();

            bullet.X = x;
            bullet.Y = y;
            bullet.Angle = angle;
            bullet.Speed = speed;
            bullet.SpriteIndex = spriteIndex;
            bullet.UpdateFunc = bulletFunc;

            return bullet;
        }
        
        private Bullet FindNextAvailableBullet()
        {
            int i;
            for (i = 0; i < MaxBullets; i++)
            {
                var trueIndex = (_availableBulletIndex + i) % MaxBullets;

                if (!_bullets[trueIndex].IsActive)
                {
                    _availableBulletIndex = trueIndex;
                    break;
                }
            }

            if (i == MaxBullets) _availableBulletIndex = (_availableBulletIndex + 1) % MaxBullets;

            return _bullets[_availableBulletIndex];
        }


        public void Update(float dt)
        {
            for (var i = 0; i < MaxBullets; i++) {
                var b = _bullets[i];

                if (!b.IsActive) {
                    continue;
                }

                b.X += (float)Math.Cos(b.Angle) * b.Speed;
                b.Y += (float)Math.Sin(b.Angle) * b.Speed;
                    
                // if the bullet's "update state" is ready
                if (_bulletWaitTimers[i] <= 0) {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain) {
                        _bulletEnumerators[i] = _bulletEnumerators[i] ?? b.Update();

                        // this steps through the bullet's update function until it hits a yield return
                        if (_bulletEnumerators[i].MoveNext()) {
                            // TODO: check the type of _bulletEnumerators[i].Current to make sure it isn't null?
                            // starting next frame, this bullet is 'waiting'
                            _bulletWaitTimers[i] = _bulletEnumerators[i].Current.Delay;

                            loopAgain = false;
                        }
                        else { // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            _bulletEnumerators[i] = b.Update();

                            loopAgain = true;
                        }
                    }
                }
                else { // the bullet is "waiting"
                    _bulletWaitTimers[i]--;
                }
            }
        }
    }
}
