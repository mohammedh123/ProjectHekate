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
                var bullet = System.FireBullet(1, 2, 3);

                // Verify: returns a bullet
                bullet.X.Should().Be(1);
                bullet.Y.Should().Be(2);
                bullet.SpriteIndex.Should().Be(3);
            }

            [TestMethod]
            public void ShouldReturnTheFirstBulletAfterFiringMaxBullets()
            {
                // Setup: fire BulletSystem.MaxBullets bullets
                IBullet firstBullet = null;
                for (int i = 0; i < BulletSystem.MaxBullets; i++) {
                    if (i == 0) firstBullet = System.FireBullet(i, i, i);
                    else {
                        System.FireBullet(i, 0, 0);
                    }
                }

                // Act: fire a bullet once more
                var lastBullet = System.FireBullet(-1, 0, 0);

                // Verify: last fired = first fired
                lastBullet.ShouldBeEquivalentTo(firstBullet);
            }

            [TestMethod]
            public void ShouldBeDifferentBullets()
            {
                // Setup: fire 1 bullet
                var firstBullet = System.FireBullet(0, 0, 0);

                // Act: fire a bullet once more
                var lastBullet = System.FireBullet(-1, 0, 0);

                // Verify: last fired != first fired
                lastBullet.X.Should().NotBe(firstBullet.X);
            }
        }
    }
}