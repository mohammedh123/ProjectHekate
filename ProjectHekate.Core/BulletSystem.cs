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

        IReadOnlyCollection<IBullet> Bullets { get; }
        IReadOnlyCollection<ICurvedLaser> CurvedLasers { get; }
    }

    public class BulletSystem : IBulletSystem
    {
        public const int MaxBullets = 2048;
        public const int MaxCurvedLasers = 64;

        // TODO: break projectiles into arrays of components

        private readonly Bullet[] _bullets = new Bullet[MaxBullets];
        private int _availableBulletIndex;

        private readonly CurvedLaser[] _curvedLasers = new CurvedLaser[MaxCurvedLasers];
        private int _availableCurvedLaserIndex;

        private readonly float[] _bulletWaitTimers = new float[MaxBullets];
        private readonly IEnumerator<WaitInFrames>[] _bulletEnumerators = new IEnumerator<WaitInFrames>[MaxBullets];

        private readonly float[] _curvedLaserWaitTimers = new float[MaxCurvedLasers];
        private readonly IEnumerator<WaitInFrames>[] _curvedLaserEnumerators = new IEnumerator<WaitInFrames>[MaxCurvedLasers];

        public IReadOnlyCollection<IBullet> Bullets { get; private set; }
        public IReadOnlyCollection<ICurvedLaser> CurvedLasers { get; private set; }

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

            Bullets = Array.AsReadOnly(_bullets);
            CurvedLasers = Array.AsReadOnly(_curvedLasers);
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

        private Bullet FindNextAvailableBullet()
        {
            return FindNextAvailableProjectile(MaxBullets, ref _availableBulletIndex, _bullets);
        }

        private CurvedLaser FindNextAvailableCurvedLaser()
        {
            return FindNextAvailableProjectile(MaxCurvedLasers, ref _availableCurvedLaserIndex, _curvedLasers);
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

    }
}
