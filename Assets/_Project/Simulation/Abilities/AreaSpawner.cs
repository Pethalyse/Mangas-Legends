using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Abilities
{
    public struct NetArea : INetworkStruct
    {
        public short Definition;
        public byte Rank;
        public NetworkBool Active;
        public NetworkBool Activated;
        public int SpawnTick;
        public int ActivateTick;
        public int NextIntervalTick;
        public int EndTick;
        public Vector3 Position;
        public Vector3 Direction;
    }

    public sealed class AreaSpawner : NetworkBehaviour
    {
        private const int Capacity = 6;

        private readonly List<Unit> _hits = new List<Unit>();
        private Unit _unit;

        [Networked] public int SpawnCount { get; set; }
        [Networked, Capacity(Capacity)] public NetworkArray<NetArea> Areas => default;

        public void Initialize(Unit unit) => _unit = unit;

        public AreaDefinition DefinitionOf(in NetArea area)
        {
            var areas = _unit.Services.Content.Areas;
            return area.Definition >= 0 && area.Definition < areas.Count ? areas[area.Definition] : null;
        }

        public void Spawn(AreaDefinition definition, int rank, Vector3 position, Vector3 direction)
        {
            short index = (short)_unit.Services.Content.Areas.IndexOf(definition.Id);
            if (index < 0) return;
            int tick = Runner.Tick;
            int rate = Runner.TickRate;
            int activate = tick + TickTime.SecondsToTicks(definition.DelaySecondsAt(rank), rate);
            direction.y = 0f;
            Areas.Set(SpawnCount % Capacity, new NetArea
            {
                Definition = index,
                Rank = (byte)rank,
                Active = true,
                SpawnTick = tick,
                ActivateTick = activate,
                NextIntervalTick = activate,
                EndTick = activate + TickTime.SecondsToTicks(definition.DurationSecondsAt(rank), rate),
                Position = position,
                Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward
            });
            SpawnCount++;
        }

        public void Simulate()
        {
            int tick = Runner.Tick;
            for (int i = 0; i < Areas.Length; i++)
            {
                NetArea area = Areas[i];
                if (!area.Active) continue;
                AreaDefinition definition = DefinitionOf(area);
                if (definition == null)
                {
                    area.Active = false;
                    Areas.Set(i, area);
                    continue;
                }
                if (definition.FollowsCaster) area.Position = _unit.Position;
                if (tick >= area.ActivateTick && !area.Activated)
                {
                    area.Activated = true;
                    if (HasStateAuthority) Apply(definition, definition.OnActivate, area);
                }
                float interval = definition.IntervalSecondsAt(area.Rank);
                if (area.Activated && interval > 0f && tick >= area.NextIntervalTick && tick < area.EndTick)
                {
                    area.NextIntervalTick = tick + Mathf.Max(1, TickTime.SecondsToTicks(interval, Runner.TickRate));
                    if (HasStateAuthority) Apply(definition, definition.OnInterval, area);
                }
                if (area.Activated && tick >= area.EndTick) area.Active = false;
                Areas.Set(i, area);
            }
        }

        private void Apply(AreaDefinition definition, IReadOnlyList<Data.Effects.EffectDefinition> effects, in NetArea area)
        {
            if (effects == null || effects.Count == 0) return;
            Collect(definition, area, _hits);
            if (_hits.Count == 0) return;
            Unit[] hits = _hits.ToArray();
            EffectExecutor.Run(effects, new EffectContext(_unit, hits[0], area.Position, area.Position, area.Rank, hits));
        }

        private void Collect(AreaDefinition definition, in NetArea area, List<Unit> results)
        {
            int rank = area.Rank;
            float reach = definition.Shape == AreaShape.Circle ? definition.RadiusAt(rank) : Mathf.Max(definition.LengthAt(rank), definition.RadiusAt(rank));
            _unit.Services.Units.Query(area.Position, reach, _unit, definition.Affects, results);
            for (int i = results.Count - 1; i >= 0; i--)
                if (!definition.Accepts(results[i].Kind)) results.RemoveAt(i);
            if (definition.Shape == AreaShape.Circle) return;
            for (int i = results.Count - 1; i >= 0; i--)
                if (!Inside(definition, area, results[i])) results.RemoveAt(i);
        }

        private static bool Inside(AreaDefinition definition, in NetArea area, Unit unit)
        {
            Vector3 offset = unit.Position - area.Position;
            offset.y = 0f;
            int rank = area.Rank;
            if (definition.Shape == AreaShape.Cone)
                return offset.magnitude <= definition.RadiusAt(rank) + unit.Radius && Vector3.Angle(area.Direction, offset) <= definition.AngleAt(rank) * 0.5f;
            float along = Vector3.Dot(offset, area.Direction);
            float across = Mathf.Abs(Vector3.Dot(offset, Vector3.Cross(Vector3.up, area.Direction)));
            return along >= -unit.Radius && along <= definition.LengthAt(rank) + unit.Radius && across <= definition.WidthAt(rank) * 0.5f + unit.Radius;
        }
    }
}
