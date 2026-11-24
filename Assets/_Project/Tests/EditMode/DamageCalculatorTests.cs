using MangaLegends.Core.Combat;
using MangaLegends.Core.Stats;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class DamageCalculatorTests
    {
        private static readonly CombatRules Rules = new CombatRules(resistanceScale: 100f, minimumDamage: 0f);

        [Test]
        public void Physical_IsReducedByArmor()
        {
            StatBlock defender = new FakeStatBaseline().With(StatType.Armor, 100f).ToBlock();

            DamageResult result = DamageCalculator.Resolve(new DamageRequest(200f, DamageType.Physical), null, defender, null, Rules);

            Assert.That(result.AfterMitigation, Is.EqualTo(100f).Within(0.001f));
            Assert.That(result.HealthDamage, Is.EqualTo(100f).Within(0.001f));
        }

        [Test]
        public void Magical_IsReducedByMagicResist()
        {
            StatBlock defender = new FakeStatBaseline().With(StatType.MagicResist, 50f).ToBlock();

            float damage = DamageCalculator.Mitigate(150f, DamageType.Magical, null, defender, Rules);

            Assert.That(damage, Is.EqualTo(100f).Within(0.001f));
        }

        [Test]
        public void True_IgnoresResistances()
        {
            StatBlock defender = new FakeStatBaseline().With(StatType.Armor, 300f).With(StatType.MagicResist, 300f).ToBlock();

            float damage = DamageCalculator.Mitigate(80f, DamageType.True, null, defender, Rules);

            Assert.That(damage, Is.EqualTo(80f));
        }

        [Test]
        public void Penetration_AppliesPercentThenFlat_AndNeverGoesBelowZero()
        {
            StatBlock attacker = new FakeStatBaseline()
                .With(StatType.ArmorPenetration, 0.5f)
                .With(StatType.Lethality, 10f)
                .ToBlock();
            StatBlock defender = new FakeStatBaseline().With(StatType.Armor, 100f).ToBlock();
            StatBlock lowArmor = new FakeStatBaseline().With(StatType.Armor, 15f).ToBlock();

            Assert.That(DamageCalculator.EffectiveResistance(DamageType.Physical, attacker, defender), Is.EqualTo(40f).Within(0.001f));
            Assert.That(DamageCalculator.EffectiveResistance(DamageType.Physical, attacker, lowArmor), Is.EqualTo(0f));
        }

        [Test]
        public void NegativeResistance_AmplifiesDamage()
        {
            Assert.That(DamageCalculator.ResistanceMultiplier(-100f, 100f), Is.EqualTo(1.5f).Within(0.001f));
        }

        [Test]
        public void ResistanceScale_ComesFromRules()
        {
            StatBlock defender = new FakeStatBaseline().With(StatType.Armor, 50f).ToBlock();

            float damage = DamageCalculator.Mitigate(100f, DamageType.Physical, null, defender, new CombatRules(50f, 0f));

            Assert.That(damage, Is.EqualTo(50f).Within(0.001f));
        }

        [Test]
        public void ShieldBrokenByHit_StillAbsorbsItsRemainingValue()
        {
            ShieldPool shields = new ShieldPool();
            shields.Add(new Shield(1, 30f, ShieldFilter.Any, expiresAtTick: 0));

            DamageResult result = DamageCalculator.Resolve(new DamageRequest(100f, DamageType.True), null, null, shields, Rules);

            Assert.That(result.AbsorbedByShields, Is.EqualTo(30f));
            Assert.That(result.HealthDamage, Is.EqualTo(70f));
            Assert.That(shields.Shields, Is.Empty);
        }

        [Test]
        public void IgnoreShieldsFlag_BypassesShields()
        {
            ShieldPool shields = new ShieldPool();
            shields.Add(new Shield(1, 30f, ShieldFilter.Any, 0));

            DamageResult result = DamageCalculator.Resolve(
                new DamageRequest(50f, DamageType.True, DamageFlags.IgnoreShields), null, null, shields, Rules);

            Assert.That(result.HealthDamage, Is.EqualTo(50f));
            Assert.That(shields.Total, Is.EqualTo(30f));
        }

        [Test]
        public void ZeroOrNegativeAmount_ReturnsNone()
        {
            DamageResult result = DamageCalculator.Resolve(new DamageRequest(-5f, DamageType.Physical), null, null, null, Rules);

            Assert.That(result.HealthDamage, Is.EqualTo(0f));
        }
    }
}
