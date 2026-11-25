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
                if (unit == null || (!includeDead && !unit.IsAlive) || !unit.IsTargetable) continue;
                if (relativeTo != null && !TeamRules.Matches(filter, relativeTo, unit)) continue;
                if (FlatDistance(center, unit.Position) <= radius + unit.Radius) results.Add(unit);
            }
            results.Sort((a, b) => a.SortKey.CompareTo(b.SortKey));
        }

        public Unit Closest(Vector3 center, float radius, Predicate<Unit> match)
        {
            Unit best = null;
            float bestDistance = float.MaxValue;
            foreach (Unit unit in _units)
            {
                if (unit == null || !unit.IsAlive || !match(unit)) continue;
                float distance = FlatDistance(center, unit.Position) - unit.Radius;
                if (distance > radius) continue;
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
