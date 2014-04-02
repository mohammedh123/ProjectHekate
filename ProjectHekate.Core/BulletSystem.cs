using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public class WaitInFrames
    {
        public int Delay { get; set; }

        public WaitInFrames(int delay)
        {
            Delay = delay;
        }
    }

    public delegate IEnumerator<WaitInFrames> UpdateDelegate(Bullet bullet);

    public interface IBullet
    {
        float X { get; }

        float Y { get; }

        float Angle { get; }

        /// <summary>
        /// The speed of the bullet (measured in pixels per frame).
        /// </summary>
        float Speed { get; }

        int SpriteIndex { get; }

        bool IsActive { get; }
    }

    public class Bullet : IBullet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public float Speed { get; set; }
        public int SpriteIndex { get; set; }

        public Bullet()
        {
            SpriteIndex = -1;
        }

        public bool IsActive { get { return SpriteIndex >= 0; } }

        public IEnumerator<WaitInFrames> Update()
        {
            return UpdateFunc != null ? UpdateFunc(this) : null;
        }

        public UpdateDelegate UpdateFunc { get; set; }
    }

    public interface IBulletSystem
    {
        IBullet FireBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex);
        IBullet FireBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex, UpdateDelegate bulletFunc);

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

        public IBullet FireBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex)
        {
            return InternalFireBullet(x, y, angle, speedPerFrame, spriteIndex);
        }

        public IBullet FireBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex, UpdateDelegate bulletFunc)
        {
            return InternalFireBullet(x, y, angle, speedPerFrame, spriteIndex, bulletFunc);
        }

        private Bullet InternalFireBullet(float x, float y, float angle, float speed, int spriteIndex, UpdateDelegate bulletFunc = null)
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
            Bullet b;
            for (var i = 0; i < MaxBullets; i++) {
                b = _bullets[i];

                if (b.IsActive)
                {
                    b.X += (float)Math.Cos(b.Angle) * b.Speed * dt;
                    b.Y += (float)Math.Sin(b.Angle) * b.Speed * dt;

                    // if the func is not waiting, call Update()
                    // if attempting to MoveNext on the Update() returns false, call Update() again
                    if (_bulletWaitTimers[i] <= 0) {
                        var loopAgain = true;

                        while (loopAgain) {
                            _bulletEnumerators[i] = _bulletEnumerators[i] ?? b.Update();
                            if (_bulletEnumerators[i].MoveNext()) {
                                if (_bulletEnumerators[i].Current is WaitForSeconds) {
                                    _bulletWaitTimers[i] = (_bulletEnumerators[i].Current as WaitForSeconds).Delay;

                                    loopAgain = false;
                                }
                                else {
                                    throw new InvalidOperationException("A bullet script has an invalid iterator return type.");
                                }
                            }
                            else {
                                _bulletEnumerators[i] = b.Update();

                                loopAgain = true;
                            }
                        }
                    }
                    else {
                        _bulletWaitTimers[i] -= dt;
                    }
                }
            }
        }
    }
}
