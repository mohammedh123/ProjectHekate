using System.Collections.Generic;

namespace ProjectHekate.Core
{
    public delegate IEnumerator<WaitInFrames> BulletUpdateDelegate(Bullet bullet);

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

        internal Bullet()
        {
            SpriteIndex = -1;
        }

        public bool IsActive { get { return SpriteIndex >= 0; } }

        internal IEnumerator<WaitInFrames> Update()
        {
            return UpdateFunc != null ? UpdateFunc(this) : null;
        }

        internal BulletUpdateDelegate UpdateFunc { get; set; }
    }
}