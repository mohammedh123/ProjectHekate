using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public delegate void UpdateDelegate(Bullet bullet);

    public interface IBullet
    {
        float X { get; }
        float Y { get; }
        float Angle { get; }
        float Speed { get; }
        int SpriteIndex { get; }

        bool IsActive { get; }

        void Update();
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

        public void Update()
        {
            if(UpdateFunc != null) UpdateFunc(this);
        }

        public UpdateDelegate UpdateFunc { get; set; }
    }

    public interface IBulletSystem
    {
        IBullet FireBullet(float x, float y, float angle, float speed, int spriteIndex);
        IBullet FireBullet(float x, float y, float angle, float speed, int spriteIndex, UpdateDelegate bulletFunc);

        IReadOnlyCollection<IBullet> Bullets { get; }

        void Update(float dt);
    }

    public class BulletSystem : IBulletSystem
    {
        public const int MaxBullets = 2048;

        // TODO: break bullets into arrays of components
        private readonly Bullet[] _bullets = new Bullet[MaxBullets];
        private int _availableBulletIndex;

        public IReadOnlyCollection<IBullet> Bullets { get; private set; }

        public BulletSystem()
        {
            for (var i = 0; i < MaxBullets; i++) {
                _bullets[i] = new Bullet();
            }

            Bullets = Array.AsReadOnly(_bullets);
        }

        public IBullet FireBullet(float x, float y, float angle, float speed, int spriteIndex)
        {
            return InternalFireBullet(x, y, angle, speed, spriteIndex);
        }

        public IBullet FireBullet(float x, float y, float angle, float speed, int spriteIndex, UpdateDelegate bulletFunc)
        {
            return InternalFireBullet(x, y, angle, speed, spriteIndex, bulletFunc);
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

                    b.Update();
                }
            }
        }
    }
}
