using MangaLegends.Core.Scaling;
using MangaLegends.Core.Simulation;
using MangaLegends.Core.Stats;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class SimulationUtilityTests
    {
        [TestCase(1f, 30, 30)]
        [TestCase(0.5f, 30, 15)]
        [TestCase(0.01f, 30, 1)]
        [TestCase(0f, 30, 0)]
        public void SecondsToTicks_RoundsUp(float seconds, int tickRate, int expected)
        {
            Assert.That(TickTime.SecondsToTicks(seconds, tickRate), Is.EqualTo(expected));
        }

        [Test]
        public void AbilityHaste_ReducesCooldownHyperbolically()
        {
            Assert.That(CooldownCalculator.ApplyHaste(10f, 100f, 100f), Is.EqualTo(5f).Within(0.001f));
            Assert.That(CooldownCalculator.ToTicks(10f, 0f, 100f, 30), Is.EqualTo(300));
        }

        [Test]
        public void DeterministicRandom_SameSeedGivesSameSequence()
        {
            DeterministicRandom a = DeterministicRandom.ForTick(42UL, 1000, 3);
            DeterministicRandom b = DeterministicRandom.ForTick(42UL, 1000, 3);

            for (int i = 0; i < 10; i++) Assert.That(a.NextULong(), Is.EqualTo(b.NextULong()));
        }

        [Test]
        public void DeterministicRandom_FloatsStayInUnitRange_AndRollRespectsBounds()
        {
            DeterministicRandom random = new DeterministicRandom(7UL);

            for (int i = 0; i < 1000; i++)
            {
                float value = random.NextFloat();
                Assert.That(value, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
            }
            Assert.That(random.Roll(0f), Is.False);
            Assert.That(random.Roll(1f), Is.True);
        }

        [Test]
        public void ScalingValue_CombinesRankBaseAndRatios()
        {
            StatBlock caster = new FakeStatBaseline().With(StatType.AttackDamage, 100f).With(StatType.AbilityPower, 0f).ToBlock();
            caster.AddModifier(new StatModifier(StatType.AttackDamage, ModifierKind.Flat, 40f, 1));
            ScalingValue value = new ScalingValue(
                new[] { 50f, 80f, 110f },
                new[]
                {
                    new ScalingRatio(ScalingSource.CasterBonusStat, 0.5f, StatType.AttackDamage),
                    new ScalingRatio(ScalingSource.TargetMissingHealth, 0.1f)
                });

            float result = value.Evaluate(2, new ScalingContext(caster, targetMaxHealth: 1000f, targetCurrentHealth: 600f));

            Assert.That(result, Is.EqualTo(80f + 20f + 40f).Within(0.001f));
        }

        [Test]
        public void ScalingValue_ClampsRank()
        {
            ScalingValue value = new ScalingValue(new[] { 10f, 20f });

            Assert.That(value.Evaluate(0, default), Is.EqualTo(10f));
            Assert.That(value.Evaluate(9, default), Is.EqualTo(20f));
        }
    }
}
