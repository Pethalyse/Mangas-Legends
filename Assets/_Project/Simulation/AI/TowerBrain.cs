using Fusion;
using MangaLegends.Core.Stats;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.AI
{
    [RequireComponent(typeof(Unit))]
    public sealed class TowerBrain : NetworkBehaviour, IUnitBrain
    {
        private Unit _unit;

        [Networked] public NetworkId RampTarget { get; set; }
        [Networked] public int RampShots { get; set; }
        [Networked] public int LastCountedAttack { get; set; }

        private void Awake() => _unit = GetComponent<Unit>();

        public void Think()
        {
            if (!HasStateAuthority || _unit.Attack == null) return;
            float range = _unit.Stats.Get(StatType.AttackRange) + _unit.Radius;
            Unit current = null;
            bool keep = _unit.TryResolve(_unit.Attack.Target, out current) && !TargetSelection.ShouldDrop(_unit, current, range);
            Unit picked = TargetSelection.Pick(_unit, range);
            bool harasser = picked != null && picked.IsChampion && picked != current && current != null && !current.IsChampion;
            if (!keep || harasser) current = picked;
            if (current == null)
            {
                _unit.Attack.ClearTarget();
                return;
            }
            _unit.Attack.SetTarget(current);
            UpdateRamp(current);
        }

        private void UpdateRamp(Unit target)
        {
            var rules = _unit.Services.Rules;
            if (target.Object.Id != RampTarget)
            {
                RampTarget = target.Object.Id;
                RampShots = 0;
            }
            if (_unit.Attack.LastAttackTick != LastCountedAttack && target.IsChampion)
            {
                LastCountedAttack = _unit.Attack.LastAttackTick;
                RampShots++;
            }
            float bonus = target.IsChampion ? Mathf.Min(rules.TowerDamageRampMax, rules.TowerDamageRampPerShot * Mathf.Max(0, RampShots - 1)) : 0f;
            _unit.Attack.DamageMultiplier = 1f + bonus;
        }
    }
}
