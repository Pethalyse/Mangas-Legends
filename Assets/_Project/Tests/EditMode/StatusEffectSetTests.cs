using MangaLegends.Core.Status;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class StatusEffectSetTests
    {
        private static readonly StatusEffectSpec Stun = new StatusEffectSpec(
            1, 30, StatusRestriction.Move | StatusRestriction.Cast | StatusRestriction.Attack, isCrowdControl: true);

        private static readonly StatusEffectSpec Mark = new StatusEffectSpec(
            2, 90, stacking: StackingRule.AddStack, maxStacks: 3);

        [Test]
        public void Stun_BlocksMovingCastingAndAttacking_UntilExpired()
        {
            StatusEffectSet set = new StatusEffectSet();

            set.Apply(Stun, sourceId: 7, currentTick: 100);

            Assert.That(set.CanMove || set.CanCast || set.CanAttack, Is.False);
            Assert.That(set.IsTargetable, Is.True);

            set.RemoveExpired(130);

            Assert.That(set.CanMove && set.CanCast && set.CanAttack, Is.True);
        }

        [Test]
        public void Refresh_ExtendsDurationWithoutAddingInstance()
        {
            StatusEffectSet set = new StatusEffectSet();
            set.Apply(Stun, 7, 100);

            StatusApplyOutcome outcome = set.Apply(Stun, 7, 120);

            Assert.That(outcome, Is.EqualTo(StatusApplyOutcome.Refreshed));
            Assert.That(set.Effects.Count, Is.EqualTo(1));
            Assert.That(set.Effects[0].ExpiresAtTick, Is.EqualTo(150));
        }

        [Test]
        public void AddStack_StacksUpToMax()
        {
            StatusEffectSet set = new StatusEffectSet();

            set.Apply(Mark, 1, 0);
            set.Apply(Mark, 1, 1);
            set.Apply(Mark, 1, 2);
            StatusApplyOutcome fourth = set.Apply(Mark, 1, 3);

            Assert.That(set.GetStacks(Mark.Id), Is.EqualTo(3));
            Assert.That(fourth, Is.EqualTo(StatusApplyOutcome.Refreshed));
        }

        [Test]
        public void Independent_CreatesSeparateInstances()
        {
            StatusEffectSpec bleed = new StatusEffectSpec(3, 60, stacking: StackingRule.Independent);
            StatusEffectSet set = new StatusEffectSet();

            set.Apply(bleed, 1, 0);
            set.Apply(bleed, 2, 30);
            set.RemoveExpired(60);

            Assert.That(set.GetStacks(bleed.Id), Is.EqualTo(1));
        }

        [Test]
        public void CrowdControl_IsBlockedByImmunity()
        {
            StatusEffectSet set = new StatusEffectSet();

            StatusApplyOutcome outcome = set.Apply(Stun, 7, 0, immuneToCrowdControl: true);

            Assert.That(outcome, Is.EqualTo(StatusApplyOutcome.BlockedByImmunity));
            Assert.That(set.Effects, Is.Empty);
        }

        [Test]
        public void PermanentEffect_NeverExpires()
        {
            StatusEffectSet set = new StatusEffectSet();
            set.Apply(new StatusEffectSpec(4, 0), 1, 0);

            set.RemoveExpired(int.MaxValue);

            Assert.That(set.Has(4), Is.True);
        }

        [Test]
        public void Removed_IsRaisedForEachRemovedEffect()
        {
            StatusEffectSet set = new StatusEffectSet();
            int removed = 0;
            set.Removed += _ => removed++;
            set.Apply(Stun, 1, 0);
            set.Apply(Mark, 1, 0);

            set.RemoveCrowdControl();

            Assert.That(removed, Is.EqualTo(1));
            Assert.That(set.Has(Mark.Id), Is.True);
        }
    }
}
