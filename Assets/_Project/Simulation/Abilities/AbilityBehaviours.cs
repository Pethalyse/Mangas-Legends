using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Abilities
{
    // Behaviour ids referenced by AbilityDefinition.BehaviourId; their parameters stay in the data.
    public static class AbilityBehaviours
    {
        public const string Recall = "recall";
        public const string PeriodicOutOfCombat = "periodic_out_of_combat";
        public const string RestoreTrinket = "restore_trinket";
        public const string OnHit = "on_hit";
        public const string OnHitChampion = "on_hit_champion";
        public const string OnAbilityHit = "on_ability_hit";
        public const string OnKill = "on_kill";
        public const string OnDamageTaken = "on_damage_taken";

        public static void OnExecuted(string behaviourId, Unit caster, Unit target, Vector3 aimPoint)
        {
            if (behaviourId == Recall && caster.HasStateAuthority) caster.Services.Match?.ReturnToBase(caster);
            if (behaviourId == RestoreTrinket) caster.Caster?.RestoreTrinket();
        }

        public static bool CanTriggerPassive(string behaviourId, Unit unit)
        {
            if (behaviourId != PeriodicOutOfCombat) return false;
            return !unit.Health.IsInCombat(unit.Services.Rules.OutOfCombatSeconds);
        }

        public static bool MatchesTrigger(string behaviourId, ItemTrigger trigger, Unit other)
        {
            switch (behaviourId)
            {
                case OnHit: return trigger == ItemTrigger.OnHit;
                case OnHitChampion: return trigger == ItemTrigger.OnHit && other != null && other.IsChampion;
                case OnAbilityHit: return trigger == ItemTrigger.OnAbilityHit;
                case OnKill: return trigger == ItemTrigger.OnKill;
                case OnDamageTaken: return trigger == ItemTrigger.OnDamageTaken;
                default: return false;
            }
        }

        public static bool IsInterruptedByDamage(string behaviourId) => behaviourId == Recall;
    }
}
