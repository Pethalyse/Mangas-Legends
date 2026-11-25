using Fusion;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Combat
{
    public sealed class ResourcePool : NetworkBehaviour
    {
        private Unit _unit;

        [Networked] public float Current { get; set; }

        public bool UsesResource => _unit != null && _unit.Definition is ChampionDefinition champion && champion.Resource == ResourceKind.Mana;
        public float Max => _unit != null ? _unit.Stats.Get(StatType.MaxMana) : 0f;
        public float Ratio => Max > 0f ? Mathf.Clamp01(Current / Max) : 0f;

        public void Initialize(Unit unit)
        {
            _unit = unit;
            if (HasStateAuthority) Current = Max;
        }

        public void Simulate()
        {
            if (!UsesResource || _unit.Health.IsDead) return;
            float regen = _unit.Stats.Get(StatType.ManaRegen);
            if (regen > 0f && Current < Max) Current = Mathf.Min(Max, Current + regen * Runner.DeltaTime);
            if (Current > Max) Current = Max;
        }

        public bool CanAfford(float cost) => !UsesResource || cost <= 0f || Current >= cost;

        public void Spend(float cost)
        {
            if (UsesResource && cost > 0f) Current = Mathf.Max(0f, Current - cost);
        }

        public float Restore(float amount)
        {
            if (!UsesResource || amount <= 0f) return 0f;
            float applied = Mathf.Min(amount, Max - Current);
            Current += applied;
            return applied;
        }

        public void Fill()
        {
            if (HasStateAuthority) Current = Max;
        }
    }
}
