using Fusion;
using Unity.Profiling;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Movement;
using MangaLegends.Simulation.Services;
using UnityEngine;

namespace MangaLegends.Simulation.Units
{
    public enum ProtectionMode : byte
    {
        None,
        WhileAnyProtectorAlive,
        WhileAllProtectorsAlive
    }

    public interface IUnitBrain
    {
        void Think();
    }

    [RequireComponent(typeof(UnitStats), typeof(Health), typeof(StatusEffects))]
    public sealed class Unit : NetworkBehaviour
    {
        private MatchServices _services;

        internal int TriggerDepth { get; set; }
        private IUnitBrain _brain;

        [Networked] public Team Team { get; set; }
        [Networked] public short DefinitionIndex { get; set; }
        [Networked] public byte Lane { get; set; }
        [Networked] public NetworkBool Fortified { get; set; }
        [Networked] public ProtectionMode Protection { get; set; }
        [Networked, Capacity(3)] public NetworkArray<NetworkId> Protectors => default;

        public UnitStats Stats { get; private set; }
        public Health Health { get; private set; }
        public StatusEffects Status { get; private set; }
        public ResourcePool Resource { get; private set; }
        public UnitMotor Motor { get; private set; }
        public AutoAttack Attack { get; private set; }
        public AbilityCaster Caster { get; private set; }

        public MatchServices Services => _services != null ? _services : _services = MatchServices.Of(Runner);
        public UnitDefinition Definition => Services != null && Services.Content != null && DefinitionIndex >= 0 && DefinitionIndex < Services.Content.Units.Count
            ? Services.Content.Units[DefinitionIndex]
            : null;
        public UnitKind Kind => Definition != null ? Definition.Kind : UnitKind.Minion;
        public bool IsChampion => Kind == UnitKind.Champion;
        public bool IsStructure => Kind == UnitKind.Tower || Kind == UnitKind.Structure;
        public bool IsMonster => Kind == UnitKind.Monster;
        public bool IsAlive => Health != null && !Health.IsDead;
        public bool IsTargetable => Status == null || Status.IsTargetable;
        public Vector3 Position => transform.position;
        public float Radius => Definition != null ? Definition.CollisionRadius : 0.5f;
        public uint SortKey => Object != null ? Object.Id.Raw : 0u;
        public int Tick => Runner.Tick;
        public int TickRate => Runner.TickRate;

        private void Awake()
        {
            Stats = GetComponent<UnitStats>();
            Health = GetComponent<Health>();
            Status = GetComponent<StatusEffects>();
            Resource = GetComponent<ResourcePool>();
            Motor = GetComponent<UnitMotor>();
            Attack = GetComponent<AutoAttack>();
            Caster = GetComponent<AbilityCaster>();
            _brain = GetComponent<IUnitBrain>();
        }

        public override void Spawned()
        {
            Services?.Units.Add(this);
            Stats.Initialize(this);
            Health.Initialize(this);
            Status.Initialize(this);
            Resource?.Initialize(this);
            Motor?.Initialize(this);
            Attack?.Initialize(this);
            Caster?.Initialize(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            MatchServices.Of(runner)?.Units.Remove(this);
        }

        private static readonly ProfilerMarker StatsMarker = new ProfilerMarker("ML.Unit.Stats");
        private static readonly ProfilerMarker StatusMarker = new ProfilerMarker("ML.Unit.Status");
        private static readonly ProfilerMarker HealthMarker = new ProfilerMarker("ML.Unit.Health");
        private static readonly ProfilerMarker BrainMarker = new ProfilerMarker("ML.Unit.Brain.Minion");
        private static readonly ProfilerMarker ChampionBrainMarker = new ProfilerMarker("ML.Unit.Brain.Champion");
        private static readonly ProfilerMarker TowerBrainMarker = new ProfilerMarker("ML.Unit.Brain.Tower");
        private static readonly ProfilerMarker MonsterBrainMarker = new ProfilerMarker("ML.Unit.Brain.Other");
        private static readonly ProfilerMarker CasterMarker = new ProfilerMarker("ML.Unit.Caster");
        private static readonly ProfilerMarker AttackMarker = new ProfilerMarker("ML.Unit.Attack");
        private static readonly ProfilerMarker MotorMarker = new ProfilerMarker("ML.Unit.Motor");

        public override void FixedUpdateNetwork()
        {
            if (Definition == null) return;
            using (StatsMarker.Auto()) Stats.Refresh();
            using (StatusMarker.Auto()) Status.Simulate();
            using (HealthMarker.Auto())
            {
                Health.Simulate();
                Resource?.Simulate();
                if (IsAlive && HasStateAuthority && IsChampion) CombatTriggers.Raise(this, ItemTrigger.Periodic, this);
            }
            if (IsAlive && _brain != null)
            {
                ProfilerMarker marker = IsChampion ? ChampionBrainMarker : Kind == UnitKind.Minion ? BrainMarker : Kind == UnitKind.Tower ? TowerBrainMarker : MonsterBrainMarker;
                using (marker.Auto()) _brain.Think();
            }
            using (CasterMarker.Auto()) Caster?.Simulate();
            using (AttackMarker.Auto()) Attack?.Simulate();
            using (MotorMarker.Auto()) Motor?.Simulate();
        }

        public bool IsInvulnerable
        {
            get
            {
                if (Protection == ProtectionMode.None) return false;
                int alive = 0;
                int total = 0;
                for (int i = 0; i < Protectors.Length; i++)
                {
                    NetworkId id = Protectors[i];
                    if (!id.IsValid) continue;
                    total++;
                    if (TryResolve(id, out Unit protector) && protector.IsAlive) alive++;
                }
                if (total == 0) return false;
                return Protection == ProtectionMode.WhileAnyProtectorAlive ? alive > 0 : alive == total;
            }
        }

        public bool TryResolve(NetworkId id, out Unit unit)
        {
            unit = null;
            if (!id.IsValid || Runner == null || !Runner.TryFindObject(id, out NetworkObject obj)) return false;
            unit = obj.GetComponent<Unit>();
            return unit != null;
        }

        public bool IsEnemyOf(Unit other) => other != null && TeamRules.AreEnemies(Team, other.Team);
    }
}
