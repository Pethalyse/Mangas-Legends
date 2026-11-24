using System;
using System.Collections.Generic;

namespace MangaLegends.Core.Status
{
    public sealed class StatusEffectSet
    {
        private readonly List<ActiveStatusEffect> _effects = new List<ActiveStatusEffect>();
        private int _nextInstanceId = 1;

        public IReadOnlyList<ActiveStatusEffect> Effects => _effects;

        public bool CanMove => !IsRestricted(StatusRestriction.Move);
        public bool CanCast => !IsRestricted(StatusRestriction.Cast);
        public bool CanAttack => !IsRestricted(StatusRestriction.Attack);
        public bool IsTargetable => !IsRestricted(StatusRestriction.BeTargeted);
        public bool CanUseItems => !IsRestricted(StatusRestriction.UseItems);

        public event Action<ActiveStatusEffect> Removed;

        public StatusApplyOutcome Apply(in StatusEffectSpec spec, int sourceId, int currentTick, bool immuneToCrowdControl = false)
        {
            if (spec.IsCrowdControl && immuneToCrowdControl) return StatusApplyOutcome.BlockedByImmunity;

            int index = spec.Stacking == StackingRule.Independent ? -1 : IndexOf(spec.Id);
            if (index < 0)
            {
                _effects.Add(new ActiveStatusEffect(_nextInstanceId++, spec, sourceId, currentTick));
                return StatusApplyOutcome.Added;
            }

            ActiveStatusEffect existing = _effects[index];
            switch (spec.Stacking)
            {
                case StackingRule.KeepExisting:
                    return StatusApplyOutcome.Ignored;
                case StackingRule.AddStack:
                    bool stacked = existing.Stacks < spec.MaxStacks;
                    if (stacked) existing.Stacks++;
                    existing.ExpiresAtTick = spec.IsPermanent ? 0 : currentTick + spec.DurationTicks;
                    _effects[index] = existing;
                    return stacked ? StatusApplyOutcome.Stacked : StatusApplyOutcome.Refreshed;
                default:
                    existing.ExpiresAtTick = spec.IsPermanent ? 0 : currentTick + spec.DurationTicks;
                    _effects[index] = existing;
                    return StatusApplyOutcome.Refreshed;
            }
        }

        public int RemoveExpired(int currentTick) => RemoveWhere(e => e.IsExpired(currentTick));

        public int RemoveBySpec(int specId) => RemoveWhere(e => e.Spec.Id == specId);

        public int RemoveCrowdControl() => RemoveWhere(e => e.Spec.IsCrowdControl);

        public bool Has(int specId) => IndexOf(specId) >= 0;

        public int GetStacks(int specId)
        {
            int total = 0;
            foreach (ActiveStatusEffect effect in _effects)
                if (effect.Spec.Id == specId) total += effect.Stacks;
            return total;
        }

        public bool IsRestricted(StatusRestriction restriction)
        {
            foreach (ActiveStatusEffect effect in _effects)
                if ((effect.Spec.Restrictions & restriction) != 0) return true;
            return false;
        }

        private int IndexOf(int specId) => _effects.FindIndex(e => e.Spec.Id == specId);

        private int RemoveWhere(Predicate<ActiveStatusEffect> match)
        {
            int removed = 0;
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (!match(_effects[i])) continue;
                ActiveStatusEffect effect = _effects[i];
                _effects.RemoveAt(i);
                removed++;
                Removed?.Invoke(effect);
            }
            return removed;
        }
    }
}
