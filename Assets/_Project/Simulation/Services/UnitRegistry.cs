using System;
using System.Collections.Generic;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Services
{
    public sealed class UnitRegistry
    {
        private readonly List<Unit> _units = new List<Unit>();
        private const float CellSize = 4f;
        private readonly Dictionary<long, List<Unit>> _cells = new Dictionary<long, List<Unit>>();
        private int _gridTick = int.MinValue;

        public IReadOnlyList<Unit> All => _units;

        public event Action<Unit> Added;
        public event Action<Unit> Removed;

        public void Add(Unit unit)
        {
            if (unit == null || _units.Contains(unit)) return;
            _units.Add(unit);
            Added?.Invoke(unit);
        }

        public void Remove(Unit unit)
        {
            if (_units.Remove(unit)) Removed?.Invoke(unit);
        }

        public void Query(Vector3 center, float radius, Unit relativeTo, TeamFilter filter, List<Unit> results, bool includeDead = false)
        {
            results.Clear();
            foreach (Unit unit in _units)
            {
                if (unit == null || (!includeDead && !unit.IsAlive)) continue;
                if (FlatDistance(center, unit.Position) > radius + unit.Radius) continue;
                if (!unit.IsTargetable || (relativeTo != null && !TeamRules.Matches(filter, relativeTo, unit))) continue;
                results.Add(unit);
            }
            results.Sort((a, b) => a.SortKey.CompareTo(b.SortKey));
        }

        // Unsorted neighbours from a grid rebuilt once per tick: for host-only steering, not for gameplay queries.
        public void Nearby(Vector3 center, float radius, int tick, List<Unit> results)
        {
            results.Clear();
            if (tick != _gridTick) RebuildGrid(tick);
            int minX = Cell(center.x - radius), maxX = Cell(center.x + radius);
            int minZ = Cell(center.z - radius), maxZ = Cell(center.z + radius);
            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                    if (_cells.TryGetValue(Key(x, z), out List<Unit> cell)) results.AddRange(cell);
        }

        private void RebuildGrid(int tick)
        {
            _gridTick = tick;
            foreach (List<Unit> cell in _cells.Values) cell.Clear();
            foreach (Unit unit in _units)
            {
                if (unit == null || unit.Object == null || !unit.IsAlive) continue;
                Vector3 p = unit.Position;
                long key = Key(Cell(p.x), Cell(p.z));
                if (!_cells.TryGetValue(key, out List<Unit> cell)) _cells[key] = cell = new List<Unit>();
                cell.Add(unit);
            }
        }

        private static int Cell(float value) => Mathf.FloorToInt(value / CellSize);

        private static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

        public Unit Closest(Vector3 center, float radius, Predicate<Unit> match)
        {
            Unit best = null;
            float bestDistance = float.MaxValue;
            foreach (Unit unit in _units)
            {
                if (unit == null || !unit.IsAlive) continue;
                float distance = FlatDistance(center, unit.Position) - unit.Radius;
                if (distance > radius || !match(unit)) continue;
                if (distance < bestDistance || (Mathf.Approximately(distance, bestDistance) && best != null && unit.SortKey < best.SortKey))
                {
                    best = unit;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public static float FlatDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
