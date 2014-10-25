using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProjectHekate.Core.MathExtras;
using ProjectHekate.Scripting;

namespace ProjectHekate.Core
{
    public delegate IEnumerator<WaitInFrames> ProjectileUpdateDelegate<in TProjectileType>(TProjectileType ap, IInterpolationSystem ins) where TProjectileType : AbstractProjectile;

    public interface IBullet
    {
        uint Id { get; }

        float X { get; }

        float Y { get; }

        /// <summary>
        /// Used as a normal angle for non-orbiting projectiles; used to determine position on orbit for orbiting projectiles.
        /// </summary>
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

    public interface ICurvedLaser : IBullet
    {
        uint Lifetime { get; }

        /// <summary>
        /// The list of coordinates of the curved laser.
        /// </summary>
        IReadOnlyCollection<Vector<float>> Coordinates { get; set; }
    }

    public interface IBeam : IBullet
    {
        /// <summary>
        /// The delay before the beam shows up immediately in full width.
        /// </summary>
        uint DelayInFrames { get; }

        float Length { get; }

        /// <summary>
        /// The total lifetime of the beam INCLUDING the startup delay.
        /// </summary>
        uint Lifetime { get; }
    }

    public interface ILaser : IBullet
    {
        /// <summary>
        /// Max length of the laser.
        /// </summary>
        float Length { get; }

        float CurrentLength { get; }
    }

    public abstract class AbstractProjectile : AbstractScriptObject
    {
        public uint Id { get; internal set; }
        public float Speed { get; set; }
        public float SpriteIndexAsFloat { get { return SpriteIndex; } set { SpriteIndex = (int)value; } }
        public int SpriteIndex { get; set; }
        public uint FramesAlive { get; set; }
        public float Radius { get; set; }
        
        // orbit-specific stuff
        public bool Orbiting { get; set; } // by making this setter public, you can enable/disable it via script, which is pretty cool
        public float OrbitDistance { get; set; }
        public float OrbitAngle { get; set; }
        public float OrbitalAngularSpeed { get; set; }
        internal IEmitter Emitter { get; set; }

        public bool IsActive { get { return SpriteIndex >= 0; } }

        internal AbstractProjectile()
        {
            SpriteIndex = -1;
        }
    }
    
    public class Bullet : AbstractProjectile, IBullet
    {
        internal IEnumerator<WaitInFrames> Update(IInterpolationSystem ins)
        {
            return UpdateFunc != null ? UpdateFunc(this, ins) : null;
        }

        virtual internal ProjectileUpdateDelegate<Bullet> UpdateFunc { get; set; }
    }

    public class CurvedLaser : AbstractProjectile, ICurvedLaser
    {
        internal const uint MaxLifetime = 64;
        private uint _lifetime;

        /// <summary>
        /// The lifetime of the laser.
        /// </summary>
        public uint Lifetime
        {
            get { return _lifetime; }
            internal set { _lifetime = System.Math.Max(2, System.Math.Min(value, MaxLifetime)); }
        }

        public IReadOnlyCollection<Vector<float>> Coordinates { get; set; }

        /// <summary>
        /// The list of coordinates of the curved laser.
        /// </summary>
        internal Vector<float>[] InternalCoordinates = new Vector<float>[MaxLifetime];

        public CurvedLaser()
        {
            Coordinates = new ReadOnlyCollection<Vector<float>>(InternalCoordinates);
        }

        internal IEnumerator<WaitInFrames> Update(IInterpolationSystem ins)
        {
            return UpdateFunc != null ? UpdateFunc(this, ins) : null;
        }

        internal ProjectileUpdateDelegate<CurvedLaser> UpdateFunc { get; set; }
    }

    public class Beam : AbstractProjectile, IBeam
    {
        public uint DelayInFrames { get; set; }
        public float Length { get; set; }
        public uint Lifetime { get; set; }

        // orbit-specific; an offset angle for the beam
        public float OrbitOffsetAngle { get; set; }
        
        internal IEnumerator<WaitInFrames> Update(IInterpolationSystem ins)
        {
            return UpdateFunc != null ? UpdateFunc(this, ins) : null;
        }

        virtual internal ProjectileUpdateDelegate<Beam> UpdateFunc { get; set; }
    }

    public class Laser : AbstractProjectile, ILaser
    {
        public float Length { get; internal set; }

        // orbit-specific; an offset angle for the laser
        public float OrbitOffsetAngle { get; set; }

        public float CurrentLength { get; internal set; }

        internal IEnumerator<WaitInFrames> Update(IInterpolationSystem ins)
        {
            return UpdateFunc != null ? UpdateFunc(this, ins) : null;
        }

        virtual internal ProjectileUpdateDelegate<Laser> UpdateFunc { get; set; }
    }
}