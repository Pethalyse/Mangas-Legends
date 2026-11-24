using MangaLegends.Core.Stats;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class StatBlockTests
    {
        [Test]
        public void Get_ReturnsBaseValue_AtLevelOne()
        {
            StatBlock stats = new FakeStatBaseline().With(StatType.AttackDamage, 60f, 3f).ToBlock();

            Assert.That(stats.Get(StatType.AttackDamage), Is.EqualTo(60f));
        }

        [Test]
        public void SetLevel_AddsGrowthForEachLevelAboveOne()
        {
            StatBlock stats = new FakeStatBaseline().With(StatType.AttackDamage, 60f, 3f).ToBlock();

            stats.SetLevel(5);

            Assert.That(stats.Get(StatType.AttackDamage), Is.EqualTo(72f));
            Assert.That(stats.GetBase(StatType.AttackDamage), Is.EqualTo(72f));
        }

        [Test]
        public void Modifiers_ApplyFlatThenPercentAddThenPercentMult()
        {
            StatBlock stats = new FakeStatBaseline().With(StatType.MoveSpeed, 300f).ToBlock();

            stats.AddModifier(new StatModifier(StatType.MoveSpeed, ModifierKind.Flat, 50f, 1));
            stats.AddModifier(new StatModifier(StatType.MoveSpeed, ModifierKind.PercentAdd, 0.1f, 2));
            stats.AddModifier(new StatModifier(StatType.MoveSpeed, ModifierKind.PercentAdd, 0.1f, 3));
            stats.AddModifier(new StatModifier(StatType.MoveSpeed, ModifierKind.PercentMult, -0.5f, 4));

            Assert.That(stats.Get(StatType.MoveSpeed), Is.EqualTo(210f).Within(0.001f));
        }

        [Test]
        public void RemoveModifiersFromSource_RemovesOnlyThatSource_EvenWithEqualValues()
        {
            StatBlock stats = new FakeStatBaseline().With(StatType.AttackDamage, 50f).ToBlock();
            stats.AddModifier(new StatModifier(StatType.AttackDamage, ModifierKind.Flat, 10f, sourceId: 1));
            stats.AddModifier(new StatModifier(StatType.AttackDamage, ModifierKind.Flat, 10f, sourceId: 2));

            int removed = stats.RemoveModifiersFromSource(1);

            Assert.That(removed, Is.EqualTo(1));
            Assert.That(stats.Get(StatType.AttackDamage), Is.EqualTo(60f));
            Assert.That(stats.Modifiers[0].SourceId, Is.EqualTo(2));
        }

        [Test]
        public void GetBonus_ReturnsDifferenceWithBase()
        {
            StatBlock stats = new FakeStatBaseline().With(StatType.AttackDamage, 50f).ToBlock();
            stats.AddModifier(new StatModifier(StatType.AttackDamage, ModifierKind.Flat, 25f, 1));

            Assert.That(stats.GetBonus(StatType.AttackDamage), Is.EqualTo(25f));
        }

        [Test]
        public void SetLevel_BelowOne_Throws()
        {
            StatBlock stats = new FakeStatBaseline().ToBlock();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => stats.SetLevel(0));
        }
    }
}
