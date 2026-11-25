using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Abilities
{
    // Behaviour ids referenced by AbilityDefinition.BehaviourId; their parameters stay in the data.
    public static class AbilityBehaviours
    {
        public const string Recall = "recall";
        public const string PeriodicOutOfCombat = "periodic_out_of_combat";

        public static void OnExecuted(string behaviourId, Unit caster, Unit target, Vector3 aimPoint)
        {
            if (behaviourId == Recall && caster.HasStateAuthority) caster.Services.Match?.ReturnToBase(caster);
        }

        public static bool CanTriggerPassive(string behaviourId, Unit unit)
        {
            if (behaviourId != PeriodicOutOfCombat) return false;
            return !unit.Health.IsInCombat(unit.Services.Rules.OutOfCombatSeconds);
        }

        public static bool IsInterruptedByDamage(string behaviourId) => behaviourId == Recall;
    }
}
