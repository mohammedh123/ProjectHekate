using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Core.MathExtras;

namespace ProjectHekate.Core
{
    public interface IBulletSystem
    {
        IBullet FireBasicBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex);
        IBullet FireScriptedBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex, ProjectileUpdateDelegate<Bullet> bulletFunc);
        ICurvedLaser FireCurvedLaser(float x, float y, float angle, float radius, uint lifetime, float speedPerFrame, int spriteIndex,
            ProjectileUpdateDelegate<CurvedLaser> laserFunc);
        IBeam FireBeam(float x, float y, float angle, float radius, uint delayInFrames, uint lifetime, int spriteIndex,
            ProjectileUpdateDelegate<Beam> beamFunc = null);
        ILaser FireLaser(float x, float y, float angle, float radius, float length, float speedPerFrame, int spriteIndex);

        void KillBullet(uint id);
        void KillCurvedLaser(uint id);
        void KillBeam(uint id);
        void KillLaser(uint id);

        IReadOnlyList<IBullet> Bullets { get; }
        IReadOnlyList<ICurvedLaser> CurvedLasers { get; }
        IReadOnlyList<IBeam> Beams { get; }
        IReadOnlyList<ILaser> Lasers { get; }
    }
    
    public class BulletSystem : IBulletSystem
    {
        private struct ProjectileData<TProjectileType> where TProjectileType : AbstractProjectile, new()
        {
            public readonly TProjectileType[] Projectiles;
            public int AvailableProjectileIndex;
            public readonly float[] ProjectileWaitTimers;
            public readonly IEnumerator<WaitInFrames>[] ProjectileEnumerators;

            public ProjectileData(uint maxProjectiles)
                : this()
            {
                Projectiles = new TProjectileType[maxProjectiles];
                AvailableProjectileIndex = 0;
                ProjectileWaitTimers = new float[maxProjectiles];
                ProjectileEnumerators = new IEnumerator<WaitInFrames>[maxProjectiles];

                for (uint i = 0; i < maxProjectiles; i++) {
                    Projectiles[i] = new TProjectileType();
                    Projectiles[i].Id = i;
                    ProjectileWaitTimers[i] = -1.0f;
                    ProjectileEnumerators[i] = null;
                }
            }

            public void KillProjectile(uint id)
            {
                // id is really just the index into the array
                Projectiles[id].SpriteIndex = -1;
            }
        }

        public const int MaxBullets = 2048;
        public const int MaxCurvedLasers = 64;
        public const int MaxBeams = 256;
        public const int MaxLasers = 1024;

        // TODO: break projectiles into arrays of components
        private ProjectileData<Bullet> _bulletData;
        private ProjectileData<CurvedLaser> _curvedLaserData;
        private ProjectileData<Beam> _beamData;
        private ProjectileData<Laser> _laserData;
        
        public IReadOnlyList<IBullet> Bullets { get; private set; }
        public IReadOnlyList<ICurvedLaser> CurvedLasers { get; private set; }
        public IReadOnlyList<IBeam> Beams { get; private set; }
        public IReadOnlyList<ILaser> Lasers { get; private set; }

        public BulletSystem()
        {
            _bulletData = new ProjectileData<Bullet>(MaxBullets);
            _curvedLaserData = new ProjectileData<CurvedLaser>(MaxCurvedLasers);
            _beamData = new ProjectileData<Beam>(MaxBeams);
            _laserData = new ProjectileData<Laser>(MaxLasers);

            Bullets = Array.AsReadOnly(_bulletData.Projectiles);
            CurvedLasers = Array.AsReadOnly(_curvedLaserData.Projectiles);
            Beams = Array.AsReadOnly(_beamData.Projectiles);
            Lasers = Array.AsReadOnly(_laserData.Projectiles);
        }

        #region Firing functions

        public IBullet FireBasicBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex)
        {
            return InternalFireBullet(x, y, angle, speedPerFrame, spriteIndex, null);
        }

        public IBullet FireScriptedBullet(float x, float y, float angle, float speedPerFrame, int spriteIndex, ProjectileUpdateDelegate<Bullet> bulletFunc)
        {
            return InternalFireBullet(x, y, angle, speedPerFrame, spriteIndex, bulletFunc);
        }

        public ICurvedLaser FireCurvedLaser(float x, float y, float angle, float radius, uint lifetime, float speedPerFrame, int spriteIndex, ProjectileUpdateDelegate<CurvedLaser> laserFunc)
        {
            return InternalFireCurvedLaser(x, y, angle, radius, lifetime, speedPerFrame, spriteIndex, laserFunc);
        }

        public IBeam FireBeam(float x, float y, float angle, float radius, uint delayInFrames, uint lifetime, int spriteIndex, ProjectileUpdateDelegate<Beam> beamFunc = null)
        {
            return InternalFireBeam(x, y, angle, radius, delayInFrames, lifetime, spriteIndex, beamFunc);
        }

        public ILaser FireLaser(float x, float y, float angle, float radius, float length, float speedPerFrame, int spriteIndex)
        {
            return InternalFireLaser(x, y, angle, radius, length, speedPerFrame, spriteIndex);
        }

        #endregion

        #region Internal firing functions & utility functions

        private Bullet InternalFireBullet(float x, float y, float angle, float speed, int spriteIndex, ProjectileUpdateDelegate<Bullet> bulletFunc)
        {
            var bullet = FindNextAvailableBullet();

            bullet.X = x;
            bullet.Y = y;
            bullet.Angle = angle;
            bullet.Speed = speed;
            bullet.SpriteIndex = spriteIndex;
            bullet.UpdateFunc = bulletFunc;
            bullet.FramesAlive = 0;

            return bullet;
        }

        private CurvedLaser InternalFireCurvedLaser(float x, float y, float angle, float radius, uint lifetime, float speed, int spriteIndex,
            ProjectileUpdateDelegate<CurvedLaser> laserFunc)
        {
            var cv = FindNextAvailableCurvedLaser();

            cv.X = x;
            cv.Y = y;
            cv.Angle = angle;
            cv.Radius = radius;
            cv.Lifetime = lifetime;
            cv.Speed = speed;
            cv.SpriteIndex = spriteIndex;
            cv.UpdateFunc = laserFunc;
            cv.FramesAlive = 0;

            return cv;
        }

        private IBeam InternalFireBeam(float x, float y, float angle, float radius, uint delayInFrames, uint lifetime, int spriteIndex, ProjectileUpdateDelegate<Beam> beamFunc)
        {
            var b = FindNextAvailableBeam();

            b.X = x;
            b.Y = y;
            b.Angle = angle;
            b.Radius = radius;
            b.DelayInFrames = delayInFrames;
            b.Lifetime = lifetime;
            b.SpriteIndex = spriteIndex;
            b.FramesAlive = 0;
            b.UpdateFunc = beamFunc;

            return b;
        }

        private ILaser InternalFireLaser(float x, float y, float angle, float radius, float length, float speedPerFrame, int spriteIndex)
        {
            var l = FindNextAvailableLaser();

            l.X = x;
            l.Y = y;
            l.Angle = angle;
            l.Radius = radius;
            l.Length = length;
            l.Speed = speedPerFrame;
            l.SpriteIndex = spriteIndex;
            l.FramesAlive = 0;
            l.CurrentLength = 0;

            return l;
        }

        private Bullet FindNextAvailableBullet()
        {
            return FindNextAvailableProjectile(MaxBullets, ref _bulletData.AvailableProjectileIndex, _bulletData.Projectiles);
        }

        private CurvedLaser FindNextAvailableCurvedLaser()
        {
            return FindNextAvailableProjectile(MaxCurvedLasers, ref _curvedLaserData.AvailableProjectileIndex, _curvedLaserData.Projectiles);
        }

        private Beam FindNextAvailableBeam()
        {
            return FindNextAvailableProjectile(MaxBeams, ref _beamData.AvailableProjectileIndex, _beamData.Projectiles);
        }

        private Laser FindNextAvailableLaser()
        {
            return FindNextAvailableProjectile(MaxLasers, ref _laserData.AvailableProjectileIndex, _laserData.Projectiles);
        }

        private TProjectileType FindNextAvailableProjectile<TProjectileType>(int maxProjectiles, ref int availableProjectileIndex, TProjectileType[] projectileArray) where TProjectileType : AbstractProjectile
        {
            int i;
            for (i = 0; i < maxProjectiles; i++)
            {
                var trueIndex = (availableProjectileIndex + i) % maxProjectiles;

                if (!projectileArray[trueIndex].IsActive)
                {
                    availableProjectileIndex = trueIndex;
                    break;
                }
            }

            if (i == maxProjectiles) availableProjectileIndex = (availableProjectileIndex + 1) % maxProjectiles;

            return projectileArray[availableProjectileIndex];
        }

        #endregion

        #region Kill projectile functions

        public void KillBullet(uint id)
        {
            _bulletData.KillProjectile(id);
        }

        public void KillCurvedLaser(uint id)
        {
            _curvedLaserData.KillProjectile(id);
        }

        public void KillBeam(uint id)
        {
            _beamData.KillProjectile(id);
        }
        public void KillLaser(uint id)
        {
            _laserData.KillProjectile(id);
        }

        #endregion

        internal void Update(float dt, IInterpolationSystem ins)
        {
            UpdateBullets(ins);
            UpdateCurvedLasers(ins);
            UpdateBeams(ins);
            UpdateLasers(ins);
        }

        private void UpdateBullets(IInterpolationSystem ins)
        {
            for (var i = 0; i < _bulletData.Projectiles.Length; i++)
            {
                var b = _bulletData.Projectiles[i];

                if (!b.IsActive)
                {
                    continue;
                }

                while (b.Angle >  Helpers.Math.TwoPi) b.Angle -= Helpers.Math.TwoPi;
                while (b.Angle < -Helpers.Math.TwoPi) b.Angle += Helpers.Math.TwoPi;

                b.X += (float)Math.Cos(b.Angle) * b.Speed;
                b.Y += (float)Math.Sin(b.Angle) * b.Speed;


                // if the bullet does not have a special update function, skip the wait logic
                if (b.UpdateFunc == null)
                {
                    b.FramesAlive++;
                    continue;
                }

                // if the bullet's "update state" is ready
                if (_bulletData.ProjectileWaitTimers[i] <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        _bulletData.ProjectileEnumerators[i] = _bulletData.ProjectileEnumerators[i] ?? b.Update(ins);

                        // this steps through the bullet's update function until it hits a yield return
                        if (_bulletData.ProjectileEnumerators[i].MoveNext())
                        {
                            // TODO: check the type of _bulletEnumerators[i].Current to make sure it isn't null?
                            // starting next frame, this bullet is 'waiting'
                            _bulletData.ProjectileWaitTimers[i] = _bulletData.ProjectileEnumerators[i].Current.Delay;

                            loopAgain = false;
                        }
                        else
                        {
                            // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            _bulletData.ProjectileEnumerators[i] = b.Update(ins);

                            loopAgain = true;
                        }
                    }
                }
                else
                {
                    // the bullet is "waiting"
                    _bulletData.ProjectileWaitTimers[i]--;
                }

                b.FramesAlive++;
            }
        }

        private void UpdateCurvedLasers(IInterpolationSystem ins)
        {
            for (var i = 0; i < _curvedLaserData.Projectiles.Length; i++)
            {
                var cv = _curvedLaserData.Projectiles[i];

                if (!cv.IsActive) {
                    continue;
                }
                
                while (cv.Angle > Helpers.Math.TwoPi) cv.Angle -= Helpers.Math.TwoPi;
                while (cv.Angle < -Helpers.Math.TwoPi) cv.Angle += Helpers.Math.TwoPi;

                cv.X += (float)Math.Cos(cv.Angle) * cv.Speed;
                cv.Y += (float)Math.Sin(cv.Angle) * cv.Speed;
                
                // if the curved laser does not have a special update function, skip the wait logic
                if (cv.UpdateFunc == null)
                {
                    cv.FramesAlive++;
                    continue;
                }

                var limit = Math.Min(cv.FramesAlive, cv.Lifetime-1);
                // add the cv's new position to the internal list of points
                // the "- 1" is so that a length of 1 means 1 point in the list of coordinates
                if (cv.FramesAlive < cv.Lifetime) {
                    // no need to do anything before appending
                }
                else {
                    // move everything over to the left and then "append"
                    Array.Copy(cv.InternalCoordinates, 1, cv.InternalCoordinates,0, cv.Lifetime-1);
                }

                //append
                cv.InternalCoordinates[limit] = new Vector<float>(cv.X, cv.Y);


                // if the curved laser's "update state" is ready
                if (_curvedLaserData.ProjectileWaitTimers[i] <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        _curvedLaserData.ProjectileEnumerators[i] = _curvedLaserData.ProjectileEnumerators[i] ?? cv.Update(ins);

                        // this steps through the curved laser's update function until it hits a yield return
                        if (_curvedLaserData.ProjectileEnumerators[i].MoveNext())
                        {
                            // TODO: check the type of _curvedLaserEnumerators[i].Current to make sure it isn't null?
                            // starting next frame, this curved laser is 'waiting'
                            _curvedLaserData.ProjectileWaitTimers[i] = _curvedLaserData.ProjectileEnumerators[i].Current.Delay;

                            loopAgain = false;
                        }
                        else
                        {
                            // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            _curvedLaserData.ProjectileEnumerators[i] = cv.Update(ins);

                            loopAgain = true;
                        }
                    }
                }
                else
                {
                    // the curved laser is "waiting"
                    _curvedLaserData.ProjectileWaitTimers[i]--;
                }

                cv.FramesAlive++;
            }
        }

        private void UpdateBeams(IInterpolationSystem ins)
        {
            for (var i = 0; i < _beamData.Projectiles.Length; i++)
            {
                var b = _beamData.Projectiles[i];

                if (!b.IsActive)
                {
                    continue;
                }

                if (b.FramesAlive > b.Lifetime) {
                    // kill this beam
                    // TODO: above
                }
                
                // if the beam does not have a special update function, skip the wait logic
                if (b.UpdateFunc == null)
                {
                    b.FramesAlive++;
                    continue;
                }

                // if the beam's "update state" is ready
                if (_beamData.ProjectileWaitTimers[i] <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        _beamData.ProjectileEnumerators[i] = _beamData.ProjectileEnumerators[i] ?? b.Update(ins);

                        // this steps through the beam's update function until it hits a yield return
                        if (_beamData.ProjectileEnumerators[i].MoveNext())
                        {
                            // TODO: check the type of _beamEnumerators[i].Current to make sure it isn't null?
                            // starting next frame, this beam is 'waiting'
                            _beamData.ProjectileWaitTimers[i] = _beamData.ProjectileEnumerators[i].Current.Delay;

                            loopAgain = false;
                        }
                        else
                        {
                            // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            _beamData.ProjectileEnumerators[i] = b.Update(ins);

                            loopAgain = true;
                        }
                    }
                }
                else
                {
                    // the beam is "waiting"
                    _beamData.ProjectileWaitTimers[i]--;
                }

                b.FramesAlive++;
            }
        }

        private void UpdateLasers(IInterpolationSystem ins)
        {
            for (var i = 0; i < _laserData.Projectiles.Length; i++)
            {
                var l = _laserData.Projectiles[i];

                if (!l.IsActive)
                {
                    continue;
                }

                l.X += (float)Math.Cos(l.Angle) * l.Speed;
                l.Y += (float)Math.Sin(l.Angle) * l.Speed;

                // update current length and cap it at max length
                l.CurrentLength = Math.Min(l.CurrentLength + l.Speed, l.Length);

                l.FramesAlive++;
            }
        }
    }
}
