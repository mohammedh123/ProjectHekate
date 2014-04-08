using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProjectHekate.Core.MathExtras;

namespace ProjectHekate.Core
{
    public delegate IEnumerator<WaitInFrames> ProjectileUpdateDelegate<in TProjectileType>(TProjectileType ap) where TProjectileType : AbstractProjectile;

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

        uint FramesAlive { get; }

        float Radius { get; }
    }

    public abstract class AbstractProjectile
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public float Speed { get; set; }
        public int SpriteIndex { get; set; }
        public uint FramesAlive { get; set; }
        public float Radius { get; set; }


        public bool IsActive { get { return SpriteIndex >= 0; } }

        internal AbstractProjectile()
        {

            SpriteIndex = -1;
        }
    }
    
    public class Bullet : AbstractProjectile, IBullet
    {
        internal IEnumerator<WaitInFrames> Update()
        {
            return UpdateFunc != null ? UpdateFunc(this) : null;
        }

        virtual internal ProjectileUpdateDelegate<Bullet> UpdateFunc { get; set; }
    }

        public bool IsActive { get { return SpriteIndex >= 0; } }

        internal IEnumerator<WaitInFrames> Update()
        {
            return UpdateFunc != null ? UpdateFunc(this) : null;
        }

        internal BulletUpdateDelegate UpdateFunc { get; set; }
    }
}