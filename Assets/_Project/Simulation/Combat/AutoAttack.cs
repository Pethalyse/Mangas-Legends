using Fusion;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Simulation;
using MangaLegends.Core.Stats;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Combat
{
    public struct AttackShot : INetworkStruct
    {
        public NetworkId Target;
        public int FireTick;
        public int HitTick;
        public float Damage;
        public NetworkBool Critical;
        public NetworkBool Active;
    }

    public sealed class AutoAttack : NetworkBehaviour
    {
        private const int ShotCapacity = 4;
        private const float RepathDistance = 0.75f;

        private Unit _unit;

        [Networked] public NetworkId Target { get; set; }
        [Networked] public int NextAttackTick { get; set; }
        [Networked] public int WindupTick { get; set; }
        [Networked] public int LastAttackTick { get; set; }
        [Networked] public int ShotCount { get; set; }
        [Networked] public float DamageMultiplier { get; set; }
        [Networked, Capacity(ShotCapacity)] public NetworkArray<AttackShot> Shots => default;

        public float Range => _unit != null ? _unit.Stats.Get(StatType.AttackRange) : 0f;
        public bool HasTarget => Target.IsValid;

        public void Initialize(Unit unit)
        {
            _unit = unit;
            if (HasStateAuthority && DamageMultiplier <= 0f) DamageMultiplier = 1f;
        }

        public void SetTarget(Unit target)
        {
            NetworkId id = target != null ? target.Object.Id : default;
            if (id == Target) return;
            Target = id;
            WindupTick = 0;
        }

        public void ClearTarget()
        {
            Target = default;
            WindupTick = 0;
        }

        public bool IsInRange(Unit target) =>
            target != null && UnitRegistry.FlatDistance(_unit.Position, target.Position) - target.Radius - _unit.Radius <= Range;

        public void Simulate()
        {
            int tick = Runner.Tick;
            ResolveShots(tick);
            if (!_unit.IsAlive)
            {
                ClearTarget();
                return;
            }
            if (!Target.IsValid) return;
            if (!_unit.TryResolve(Target, out Unit target) || !target.IsAlive || !target.IsTargetable || !_unit.IsEnemyOf(target))
            {
                ClearTarget();
                return;
            }

            if (WindupTick > 0)
            {
                if (tick >= WindupTick) Fire(target, tick);
                return;
            }

            if (!IsInRange(target))
            {
                Chase(target);
                return;
            }

            _unit.Motor?.Stop();
            _unit.Motor?.Face(target.Position);
            if (!_unit.Status.CanAttack || tick < NextAttackTick || _unit.Caster != null && _unit.Caster.IsChanneling) return;

            float attackSpeed = Mathf.Max(0.1f, _unit.Stats.Get(StatType.AttackSpeed));
            int period = Mathf.Max(1, Mathf.RoundToInt(Runner.TickRate / attackSpeed));
            int windup = Mathf.Max(1, Mathf.RoundToInt(period * _unit.Definition.AttackWindup));
            LastAttackTick = tick;
            WindupTick = tick + windup;
            NextAttackTick = tick + period;
        }

        private void Chase(Unit target)
        {
            if (_unit.Motor == null || !_unit.Status.CanMove) return;
            Vector3 destination = _unit.Motor.Destination;
            bool stale = !_unit.Motor.IsMoving || UnitRegistry.FlatDistance(destination, target.Position) > RepathDistance;
            if (stale) _unit.Motor.MoveTo(target.Position, Range + target.Radius + _unit.Radius - 0.1f);
        }

        private void Fire(Unit target, int tick)
        {
            WindupTick = 0;
            float damage = _unit.Stats.Get(StatType.AttackDamage) * DamageMultiplier;
            bool critical = false;
            if (HasStateAuthority)
            {
                DeterministicRandom random = DeterministicRandom.ForTick(_unit.Services.Match != null ? _unit.Services.Match.Seed : 0UL, tick, (int)_unit.SortKey);
                critical = random.Roll(_unit.Stats.Get(StatType.CritChance));
            }
            if (critical) damage *= Mathf.Max(1f, _unit.Stats.Get(StatType.CritDamage));

            float speed = _unit.Definition.AttackProjectileSpeed;
            if (speed <= 0f)
            {
                Hit(target, damage, critical);
                return;
            }
            float distance = UnitRegistry.FlatDistance(_unit.Position, target.Position);
            int travel = Mathf.Max(1, TickTime.SecondsToTicks(distance / speed, Runner.TickRate));
            Shots.Set(ShotCount % ShotCapacity, new AttackShot { Target = target.Object.Id, FireTick = tick, HitTick = tick + travel, Damage = damage, Critical = critical, Active = true });
            ShotCount++;
        }

        private void ResolveShots(int tick)
        {
            for (int i = 0; i < Shots.Length; i++)
            {
                AttackShot shot = Shots[i];
                if (!shot.Active || tick < shot.HitTick) continue;
                shot.Active = false;
                Shots.Set(i, shot);
                if (_unit.TryResolve(shot.Target, out Unit target) && target.IsAlive) Hit(target, shot.Damage, shot.Critical);
            }
        }

        private void Hit(Unit target, float damage, bool critical)
        {
            if (!HasStateAuthority) return;
            DamageFlags flags = DamageFlags.AutoAttack | (critical ? DamageFlags.Critical : DamageFlags.None);
            target.Health.ApplyDamage(new DamageInfo(_unit, damage, DamageType.Physical, flags));
            float lifeSteal = _unit.Stats.Get(StatType.LifeSteal);
            if (lifeSteal > 0f) _unit.Health.Heal(damage * lifeSteal, _unit);
        }
    }
}
