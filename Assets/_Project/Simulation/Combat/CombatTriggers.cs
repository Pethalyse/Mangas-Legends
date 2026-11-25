using System.Collections.Generic;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Effects;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Units;

namespace MangaLegends.Simulation.Combat
{
    // Authority-only dispatch of combat events to item passives, status triggers and trigger passives.
    // Effects run by a trigger are procs: they cannot raise another trigger of the same owner, which bounds
    // chains such as two units reflecting damage at each other.
    public static class CombatTriggers
    {
        public static void Raise(Unit owner, ItemTrigger trigger, Unit other)
        {
            if (owner == null || owner.Object == null || !owner.HasStateAuthority || owner.TriggerDepth > 0) return;
            if (!owner.IsAlive && trigger != ItemTrigger.OnKill) return;
            owner.TriggerDepth++;
            try
            {
                owner.GetComponent<Inventory>()?.RaiseTriggers(trigger, other);
                RaiseStatuses(owner, trigger, other);
                owner.Caster?.RaisePassiveTrigger(trigger, other);
            }
            finally
            {
                owner.TriggerDepth--;
            }
        }

        public static void Run(IReadOnlyList<EffectDefinition> effects, Unit owner, Unit other, int rank)
        {
            Unit target = other != null && other.IsAlive ? other : owner;
            EffectExecutor.Run(effects, new EffectContext(owner, target, target.Position, target.Position, rank, null).AsProc());
        }

        private static void RaiseStatuses(Unit owner, ItemTrigger trigger, Unit other)
        {
            StatusEffects status = owner.Status;
            for (int slot = 0; slot < status.Active.Length; slot++)
            {
                StatusEffectDefinition definition = status.DefinitionAt(slot);
                if (definition == null) continue;
                foreach (TriggeredEffect triggered in definition.Triggers)
                {
                    if (triggered == null || triggered.Trigger != trigger) continue;
                    if (triggered.OtherKinds.Count > 0 && (other == null || !triggered.Accepts(other.Kind))) continue;
                    if (!status.TryStartTriggerCooldown(slot, definition.TriggerCooldownSeconds)) break;
                    Run(triggered.Effects, owner, other, status.Active[slot].Rank);
                    if (!definition.RemovedOnTrigger) continue;
                    status.RemoveAt(slot);
                    break;
                }
            }
        }
    }
}
