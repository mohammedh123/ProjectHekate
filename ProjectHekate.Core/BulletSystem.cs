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
        int SpriteIndex { get; }

        bool IsActive { get; }

        void Update();
    }

    public class Bullet : IBullet
    {
        public float X { get; set; }
        public float Y { get; set; }
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
        IBullet FireBullet(float x, float y, int spriteIndex);
        IBullet FireBullet(float x, float y, int spriteIndex, UpdateDelegate bulletFunc);

        IReadOnlyCollection<IBullet> Bullets { get; } 
    }

    public class BulletSystem : IBulletSystem
    {
        public const int MaxBullets = 2048;

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

        public IBullet FireBullet(float x, float y, int spriteIndex)
        {
            return InternalFireBullet(x, y, spriteIndex);
        }

        public IBullet FireBullet(float x, float y, int spriteIndex, UpdateDelegate bulletFunc)
        {
            var bullet = InternalFireBullet(x, y, spriteIndex);
            bullet.UpdateFunc = bulletFunc;

            return bullet;
        }

        private Bullet InternalFireBullet(float x, float y, int spriteIndex)
        {
            var bullet = FindNextAvailableBullet();

            bullet.X = x;
            bullet.Y = y;
            bullet.SpriteIndex = spriteIndex;

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
    }
}
