using Fusion;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.AI
{
    // Summoned battering ram (the genre's herald): runs to the closest enemy tower it can hurt and hits it once
    // for its attack damage as true damage, then disappears. Towers and minions may kill it on the way.
    public sealed class ChargerBrain : NetworkBehaviour, IUnitBrain
    {
        private const float RepathSeconds = 0.5f;
        private const float ImpactMargin = 0.6f;

        private Unit _unit;

        [Networked] public NetworkId Owner { get; set; }
        [Networked] public NetworkId Target { get; set; }
        [Networked] public int NextRepathTick { get; set; }

        private void Awake() => _unit = GetComponent<Unit>();

        public void Launch(Unit owner) => Owner = owner != null ? owner.Object.Id : default;

        public void Think()
        {
            if (!HasStateAuthority) return;
            if (!_unit.TryResolve(Target, out Unit tower) || !tower.IsAlive || tower.IsInvulnerable)
            {
                tower = ClosestTower();
                if (tower == null)
                {
                    _unit.Health.Expire();
                    return;
                }
                Target = tower.Object.Id;
                NextRepathTick = 0;
            }

            float reach = tower.Radius + _unit.Radius + ImpactMargin;
            if (UnitRegistry.FlatDistance(_unit.Position, tower.Position) <= reach)
            {
                tower.Health.ApplyDamage(new DamageInfo(_unit, _unit.Stats.Get(StatType.AttackDamage), DamageType.True, DamageFlags.Proc));
                _unit.Health.Expire();
                return;
            }
            if (Runner.Tick < NextRepathTick || _unit.Motor == null) return;
            NextRepathTick = Runner.Tick + TickTime(RepathSeconds);
            _unit.Motor.MoveTo(tower.Position, reach - ImpactMargin * 0.5f);
        }

        private int TickTime(float seconds) => Core.Simulation.TickTime.SecondsToTicks(seconds, Runner.TickRate);

        private Unit ClosestTower() =>
            _unit.Services.Units.Closest(_unit.Position, 500f, u => u.Kind == UnitKind.Tower && u.IsAlive && !u.IsInvulnerable && _unit.IsEnemyOf(u));
    }
}
