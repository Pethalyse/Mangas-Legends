using MangaLegends.Core.Combat;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class ShieldPoolTests
    {
        [Test]
        public void TypedShield_IsConsumedBeforeGenericShield()
        {
            ShieldPool pool = new ShieldPool();
            pool.Add(new Shield(1, 50f, ShieldFilter.Any, 0));
            pool.Add(new Shield(2, 40f, ShieldFilter.Magical, 0));

            float absorbed = pool.Absorb(60f, DamageType.Magical);

            Assert.That(absorbed, Is.EqualTo(60f));
            Assert.That(pool.Shields.Count, Is.EqualTo(1));
            Assert.That(pool.Shields[0].Id, Is.EqualTo(1));
            Assert.That(pool.Shields[0].Remaining, Is.EqualTo(30f));
        }

        [Test]
        public void TypedShield_IgnoresOtherDamageTypes()
        {
            ShieldPool pool = new ShieldPool();
            pool.Add(new Shield(1, 50f, ShieldFilter.Magical, 0));

            float absorbed = pool.Absorb(20f, DamageType.Physical);

            Assert.That(absorbed, Is.EqualTo(0f));
            Assert.That(pool.Total, Is.EqualTo(50f));
        }

        [Test]
        public void RemoveExpired_RemovesOnlyExpiredShields()
        {
            ShieldPool pool = new ShieldPool();
            pool.Add(new Shield(1, 10f, ShieldFilter.Any, expiresAtTick: 100));
            pool.Add(new Shield(2, 10f, ShieldFilter.Any, expiresAtTick: 200));
            pool.Add(new Shield(3, 10f, ShieldFilter.Any, expiresAtTick: 0));

            pool.RemoveExpired(150);

            Assert.That(pool.Shields.Count, Is.EqualTo(2));
            Assert.That(pool.Remove(1), Is.False);
        }

        [Test]
        public void BlocksCrowdControl_IsTrueWhileSuchShieldIsActive()
        {
            ShieldPool pool = new ShieldPool();
            pool.Add(new Shield(1, 10f, ShieldFilter.Magical, 0, blocksCrowdControl: true));

            Assert.That(pool.BlocksCrowdControl, Is.True);

            pool.Absorb(10f, DamageType.Magical);

            Assert.That(pool.BlocksCrowdControl, Is.False);
        }
    }
}
