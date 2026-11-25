using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.AI
{
    [RequireComponent(typeof(Unit))]
    public sealed class MonsterBrain : NetworkBehaviour, IUnitBrain
    {
        private const float HomeReached = 0.5f;

        private Unit _unit;

        [Networked] public Vector3 Home { get; set; }
        [Networked] public float Leash { get; set; }
        [Networked] public NetworkBool Returning { get; set; }

        private void Awake() => _unit = GetComponent<Unit>();

        public void SetHome(Vector3 home, float leash)
        {
            Home = home;
            Leash = leash;
        }

        public void Think()
        {
            if (!HasStateAuthority) return;
            if (Returning)
            {
                if (UnitRegistry.FlatDistance(_unit.Position, Home) <= HomeReached || !_unit.Motor.IsMoving)
                {
                    Returning = false;
                    _unit.Health.SetCurrent(_unit.Health.Max);
                }
                return;
            }

            bool tooFar = UnitRegistry.FlatDistance(_unit.Position, Home) > Leash;
            int calm = TickTime.SecondsToTicks(_unit.Services.Rules.MonsterLeashResetSeconds, Runner.TickRate);
            bool forgotten = _unit.Health.LastDamagedTick > 0 && Runner.Tick - _unit.Health.LastDamagedTick > calm;
            if (tooFar || _unit.Attack.HasTarget && forgotten)
            {
                GoHome();
                return;
            }
            bool provoked = _unit.Health.LastDamagedTick > 0 && !forgotten;
            if (provoked && !_unit.Attack.HasTarget
                && _unit.TryResolve(_unit.Health.LastAttacker, out Unit attacker) && TargetSelection.IsValidEnemy(_unit, attacker))
                _unit.Attack.SetTarget(attacker);
        }

        private void GoHome()
        {
            _unit.Attack.ClearTarget();
            Returning = true;
            _unit.Motor.MoveTo(Home);
        }
    }
}
