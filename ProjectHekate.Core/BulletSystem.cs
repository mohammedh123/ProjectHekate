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

        IReadOnlyCollection<IBullet> Bullets { get; }
        IReadOnlyCollection<ICurvedLaser> CurvedLasers { get; }
        IReadOnlyCollection<IBeam> Beams { get; }
        IReadOnlyCollection<ILaser> Lasers { get; }
    }

    public class BulletSystem : IBulletSystem
    {
        public const int MaxBullets = 2048;
        public const int MaxCurvedLasers = 64;
        public const int MaxBeams = 256;
        public const int MaxLasers = 1024;

        // TODO: break projectiles into arrays of components
        // TODO: organize the fuck outta this
        private readonly Bullet[] _bullets = new Bullet[MaxBullets];
        private int _availableBulletIndex;

        private readonly CurvedLaser[] _curvedLasers = new CurvedLaser[MaxCurvedLasers];
        private int _availableCurvedLaserIndex;

        private readonly Beam[] _beams = new Beam[MaxBeams];
        private int _availableBeamIndex;

        private readonly Laser[] _lasers = new Laser[MaxLasers];
        private int _availableLaserIndex;

        private readonly float[] _bulletWaitTimers = new float[MaxBullets];
        private readonly IEnumerator<WaitInFrames>[] _bulletEnumerators = new IEnumerator<WaitInFrames>[MaxBullets];

        private readonly float[] _curvedLaserWaitTimers = new float[MaxCurvedLasers];
        private readonly IEnumerator<WaitInFrames>[] _curvedLaserEnumerators = new IEnumerator<WaitInFrames>[MaxCurvedLasers];

        private readonly float[] _beamWaitTimers = new float[MaxBeams];
        private readonly IEnumerator<WaitInFrames>[] _beamEnumerators = new IEnumerator<WaitInFrames>[MaxBeams];

        private readonly float[] _laserWaitTimers = new float[MaxLasers];
        private readonly IEnumerator<WaitInFrames>[] _laserEnumerators = new IEnumerator<WaitInFrames>[MaxLasers];

        public IReadOnlyCollection<IBullet> Bullets { get; private set; }
        public IReadOnlyCollection<ICurvedLaser> CurvedLasers { get; private set; }
        public IReadOnlyCollection<IBeam> Beams { get; private set; }
        public IReadOnlyCollection<ILaser> Lasers { get; private set; }

        public BulletSystem()
        {
            for (var i = 0; i < MaxBullets; i++)
            {
                _bullets[i] = new Bullet();
                _bulletWaitTimers[i] = -1.0f;
                _bulletEnumerators[i] = null;
            }

            for (var i = 0; i < MaxCurvedLasers; i++)
            {
                _curvedLasers[i] = new CurvedLaser();
                _curvedLaserWaitTimers[i] = -1.0f;
                _curvedLaserEnumerators[i] = null;
            }

            for (var i = 0; i < MaxBeams; i++)
            {
                _beams[i] = new Beam();
                _beamWaitTimers[i] = -1.0f;
                _beamEnumerators[i] = null;
            }

            for (var i = 0; i < MaxLasers; i++)
            {
                _lasers[i] = new Laser();
                _laserWaitTimers[i] = -1.0f;
                _laserEnumerators[i] = null;
            }

            Bullets = Array.AsReadOnly(_bullets);
            CurvedLasers = Array.AsReadOnly(_curvedLasers);
            Beams = Array.AsReadOnly(_beams);
            Lasers = Array.AsReadOnly(_lasers);
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
            return FindNextAvailableProjectile(MaxBullets, ref _availableBulletIndex, _bullets);
        }

        private CurvedLaser FindNextAvailableCurvedLaser()
        {
            return FindNextAvailableProjectile(MaxCurvedLasers, ref _availableCurvedLaserIndex, _curvedLasers);
        }

        private Beam FindNextAvailableBeam()
        {
            return FindNextAvailableProjectile(MaxBeams, ref _availableBeamIndex, _beams);
        }

        private Laser FindNextAvailableLaser()
        {
            return FindNextAvailableProjectile(MaxLasers, ref _availableLaserIndex, _lasers);
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

        internal void Update(float dt)
        {
            UpdateBullets();
            UpdateCurvedLasers();
            UpdateBeams();
            UpdateLasers();
        }

        private void UpdateBullets()
        {
            for (var i = 0; i < MaxBullets; i++)
            {
                var b = _bullets[i];

                if (!b.IsActive)
                {
                    continue;
                }

                b.X += (float)Math.Cos(b.Angle) * b.Speed;
                b.Y += (float)Math.Sin(b.Angle) * b.Speed;


                // if the bullet does not have a special update function, skip the wait logic
                if (b.UpdateFunc == null)
                {
                    b.FramesAlive++;
                    continue;
                }

                // if the bullet's "update state" is ready
                if (_bulletWaitTimers[i] <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        _bulletEnumerators[i] = _bulletEnumerators[i] ?? b.Update();

                        // this steps through the bullet's update function until it hits a yield return
                        if (_bulletEnumerators[i].MoveNext())
                        {
                            // TODO: check the type of _bulletEnumerators[i].Current to make sure it isn't null?
                            // starting next frame, this bullet is 'waiting'
                            _bulletWaitTimers[i] = _bulletEnumerators[i].Current.Delay;

                            loopAgain = false;
                        }
                        else
                        {
                            // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            _bulletEnumerators[i] = b.Update();

                            loopAgain = true;
                        }
                    }
                }
                else
                {
                    // the bullet is "waiting"
                    _bulletWaitTimers[i]--;
                }

                b.FramesAlive++;
            }
        }

        private void UpdateCurvedLasers()
        {
            for (var i = 0; i < MaxCurvedLasers; i++)
            {
                var cv = _curvedLasers[i];

                if (!cv.IsActive) {
                    continue;
                }

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
                if (_curvedLaserWaitTimers[i] <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        _curvedLaserEnumerators[i] = _curvedLaserEnumerators[i] ?? cv.Update();

                        // this steps through the curved laser's update function until it hits a yield return
                        if (_curvedLaserEnumerators[i].MoveNext())
                        {
                            // TODO: check the type of _curvedLaserEnumerators[i].Current to make sure it isn't null?
                            // starting next frame, this curved laser is 'waiting'
                            _curvedLaserWaitTimers[i] = _curvedLaserEnumerators[i].Current.Delay;

                            loopAgain = false;
                        }
                        else
                        {
                            // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            _curvedLaserEnumerators[i] = cv.Update();

                            loopAgain = true;
                        }
                    }
                }
                else
                {
                    // the curved laser is "waiting"
                    _curvedLaserWaitTimers[i]--;
                }

                cv.FramesAlive++;
            }
        }

        private void UpdateBeams()
        {
            for (var i = 0; i < MaxBeams; i++)
            {
                var b = _beams[i];

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
                if (_beamWaitTimers[i] <= 0)
                {
                    var loopAgain = true;

                    // this loopAgain variable basically means: start from the beginning of the Update function if the function reaches completion
                    // this means that if there isn't a yield return, the function will loop infinitely
                    // TODO: somehow prevent that
                    while (loopAgain)
                    {
                        _beamEnumerators[i] = _beamEnumerators[i] ?? b.Update();

                        // this steps through the beam's update function until it hits a yield return
                        if (_beamEnumerators[i].MoveNext())
                        {
                            // TODO: check the type of _beamEnumerators[i].Current to make sure it isn't null?
                            // starting next frame, this beam is 'waiting'
                            _beamWaitTimers[i] = _beamEnumerators[i].Current.Delay;

                            loopAgain = false;
                        }
                        else
                        {
                            // if it returns false, then it has hit the end of the function -- so loop again, from the beginning
                            _beamEnumerators[i] = b.Update();

                            loopAgain = true;
                        }
                    }
                }
                else
                {
                    // the beam is "waiting"
                    _beamWaitTimers[i]--;
                }

                b.FramesAlive++;
            }
        }

        private void UpdateLasers()
        {
            for (var i = 0; i < MaxLasers; i++)
            {
                var l = _lasers[i];

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
