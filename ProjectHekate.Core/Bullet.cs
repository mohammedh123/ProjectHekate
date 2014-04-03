using System.Collections.Generic;

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

        internal Bullet()
        {
            SpriteIndex = -1;
        }

        public bool IsActive { get { return SpriteIndex >= 0; } }

        internal IEnumerator<WaitInFrames> Update()
        {
            return UpdateFunc != null ? UpdateFunc(this) : null;
        }

        internal UpdateDelegate UpdateFunc { get; set; }
    }
}