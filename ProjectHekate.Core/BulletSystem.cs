using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Core
{
    public interface IBullet
    {
        float X { get; }
        float Y { get; }
        int SpriteIndex { get; }
    }

    class Bullet : IBullet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int SpriteIndex { get; set; }

        public Bullet()
        {
            SpriteIndex = -1;
        }

        public bool IsActive { get { return SpriteIndex >= 0; } }
    }

    public interface IBulletSystem
    {
        IBullet FireBullet(float x, float y, int spriteIndex);
    }

    public class BulletSystem : IBulletSystem
    {
        public const int MaxBullets = 2048;

        private readonly Bullet[] _bullets = new Bullet[MaxBullets];
        private int _availableBulletIndex;

        public BulletSystem()
        {
            for (var i = 0; i < MaxBullets; i++) {
                _bullets[i] = new Bullet();
            }
        }

        public IBullet FireBullet(float x, float y, int spriteIndex)
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
