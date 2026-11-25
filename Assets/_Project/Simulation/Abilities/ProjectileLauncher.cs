using System.Collections.Generic;
using Fusion;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Abilities
{
    public struct NetProjectile : INetworkStruct
    {
        public short Definition;
        public byte Rank;
        public byte Hits;
        public NetworkBool Active;
        public int SpawnTick;
        public Vector3 Position;
        public Vector3 Direction;
        public float Traveled;
        public NetworkId Homing;
        public NetworkId Hit0, Hit1, Hit2, Hit3;
    }

    // Circular buffer of the caster's projectiles: predicted by the caster, hits decided by the state authority.
    public sealed class ProjectileLauncher : NetworkBehaviour
    {
        private const int Capacity = 12;
        private const float LaunchHeight = 1f;

        private readonly List<Unit> _candidates = new List<Unit>();
        private Unit _unit;

        [Networked] public int LaunchCount { get; set; }
        [Networked, Capacity(Capacity)] public NetworkArray<NetProjectile> Projectiles => default;

        public void Initialize(Unit unit) => _unit = unit;

        public ProjectileDefinition DefinitionOf(in NetProjectile projectile)
        {
            var projectiles = _unit.Services.Content.Projectiles;
            return projectile.Definition >= 0 && projectile.Definition < projectiles.Count ? projectiles[projectile.Definition] : null;
        }

        public void Launch(ProjectileDefinition definition, int rank, Vector3 origin, Vector3 direction, Unit homing)
        {
            short index = (short)_unit.Services.Content.Projectiles.IndexOf(definition.Id);
            if (index < 0) return;
            direction.y = 0f;
            Projectiles.Set(LaunchCount % Capacity, new NetProjectile
            {
                Definition = index,
                Rank = (byte)rank,
                Active = true,
                SpawnTick = Runner.Tick,
                Position = origin + Vector3.up * LaunchHeight,
                Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward,
                Homing = homing != null ? homing.Object.Id : default
            });
            LaunchCount++;
        }

        public void Simulate()
        {
            for (int i = 0; i < Projectiles.Length; i++)
            {
                NetProjectile projectile = Projectiles[i];
                if (!projectile.Active) continue;
                ProjectileDefinition definition = DefinitionOf(projectile);
                if (definition == null)
                {
                    projectile.Active = false;
                    Projectiles.Set(i, projectile);
                    continue;
                }
                Step(ref projectile, definition);
                Projectiles.Set(i, projectile);
            }
        }

        private void Step(ref NetProjectile projectile, ProjectileDefinition definition)
        {
            int rank = projectile.Rank;
            float step = definition.SpeedAt(rank) * Runner.DeltaTime;
            Unit homing = null;
            if (projectile.Homing.IsValid && (!_unit.TryResolve(projectile.Homing, out homing) || !homing.IsAlive))
            {
                projectile.Active = false;
                return;
            }
            if (homing != null)
            {
                Vector3 toTarget = homing.Position + Vector3.up - projectile.Position;
                toTarget.y = 0f;
                if (toTarget.magnitude <= step + homing.Radius + definition.RadiusAt(rank))
                {
                    projectile.Position = homing.Position + Vector3.up;
                    projectile.Active = false;
                    if (HasStateAuthority) Impact(definition, rank, homing, projectile.Position);
                    return;
                }
                projectile.Direction = toTarget.normalized;
            }

            Vector3 from = projectile.Position;
            projectile.Position += projectile.Direction * step;
            projectile.Traveled += step;
            if (homing == null && HasStateAuthority) CheckHits(ref projectile, definition, from);
            if (projectile.Active && projectile.Traveled >= definition.MaxDistanceAt(rank) && homing == null) projectile.Active = false;
        }

        private void CheckHits(ref NetProjectile projectile, ProjectileDefinition definition, Vector3 from)
        {
            int rank = projectile.Rank;
            float radius = definition.RadiusAt(rank);
            Vector3 to = projectile.Position;
            Vector3 middle = (from + to) * 0.5f;
            float reach = UnitRegistry.FlatDistance(from, to) * 0.5f + radius + 2f;
            _unit.Services.Units.Query(middle, reach, _unit, definition.Hits, _candidates);
            foreach (Unit candidate in _candidates)
            {
                if (!projectile.Active) break;
                if (AlreadyHit(projectile, candidate.Object.Id)) continue;
                if (SegmentDistance(from, to, candidate.Position) > radius + candidate.Radius) continue;
                RememberHit(ref projectile, candidate.Object.Id);
                projectile.Hits++;
                Impact(definition, rank, candidate, candidate.Position);
                if (projectile.Hits >= definition.MaxHitsAt(rank)) projectile.Active = false;
            }
        }

        private void Impact(ProjectileDefinition definition, int rank, Unit target, Vector3 point)
        {
            var hits = new[] { target };
            EffectContext context = new EffectContext(_unit, target, point, point, rank, hits);
            EffectExecutor.Run(definition.OnHit, context);
        }

        private static bool AlreadyHit(in NetProjectile projectile, NetworkId id) =>
            projectile.Hit0 == id || projectile.Hit1 == id || projectile.Hit2 == id || projectile.Hit3 == id;

        private static void RememberHit(ref NetProjectile projectile, NetworkId id)
        {
            switch (projectile.Hits % 4)
            {
                case 0: projectile.Hit0 = id; break;
                case 1: projectile.Hit1 = id; break;
                case 2: projectile.Hit2 = id; break;
                default: projectile.Hit3 = id; break;
            }
        }

        private static float SegmentDistance(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector2 a2 = new Vector2(a.x, a.z);
            Vector2 b2 = new Vector2(b.x, b.z);
            Vector2 p2 = new Vector2(point.x, point.z);
            Vector2 ab = b2 - a2;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p2 - a2, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(a2 + ab * t, p2);
        }
    }
}
