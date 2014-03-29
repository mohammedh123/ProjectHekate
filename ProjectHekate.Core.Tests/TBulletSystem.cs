using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectHekate.Core.Tests
{
    [TestClass]
    public class TBulletSystem
    {
        protected BulletSystem System;

        [TestInitialize]
        public void Initialize()
        {
            System = new BulletSystem();
        }

        [TestClass]
        public class FireBullet : TBulletSystem
        {
            [TestMethod]
            public void ShouldReturnNextBulletInArray()
            {
                // Setup: none

                // Act: call method
                var bullet = System.FireBullet(1, 2, 3, 4, 5);

                // Verify: returns a bullet
                bullet.X.Should().Be(1);
                bullet.Y.Should().Be(2);
                bullet.Angle.Should().Be(3);
                bullet.Speed.Should().Be(4);
                bullet.SpriteIndex.Should().Be(5);
            }

            [TestMethod]
            public void ShouldReturnTheFirstBulletAfterFiringMaxBullets()
            {
                // Setup: fire BulletSystem.MaxBullets bullets
                IBullet firstBullet = null;
                for (int i = 0; i < BulletSystem.MaxBullets; i++) {
                    if (i == 0) firstBullet = System.FireBullet(i, i, i, i, i);
                    else {
                        System.FireBullet(i, 0, 0, 0, 0);
                    }
                }

                // Act: fire a bullet once more
                var lastBullet = System.FireBullet(-1, 0, 0, 0, 0);

                // Verify: last fired = first fired
                lastBullet.ShouldBeEquivalentTo(firstBullet);
            }

            [TestMethod]
            public void ShouldBeDifferentBullets()
            {
                // Setup: fire 1 bullet
                var firstBullet = System.FireBullet(0, 0, 0, 0, 0);

                // Act: fire a bullet once more
                var lastBullet = System.FireBullet(-1, 0, 0, 0, 0);

                // Verify: last fired != first fired
                lastBullet.X.Should().NotBe(firstBullet.X);
            }
            
            [TestMethod]
            public void ShouldProperlyAddUpdateFuncToBullet()
            {
                // Setup: none

                // Act: call method with updatefunc
                var bullet = System.FireBullet(0, 0, 0, 0, 0, b => b.Angle += 1);
                
                // Verify: bullet has updatefunc and it gets updated after calling update
                bullet.Angle.Should().Be(0);
                
                bullet.Update();

                bullet.Angle.Should().Be(1);
            }

            
        }
    }
}