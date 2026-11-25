using System.Collections.Generic;
using MangaLegends.Core.Stats;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Services
{
    // What the local player's team sees this frame: a unit is visible if it is an ally, a structure,
    // or stands inside the vision radius of a living allied unit. Display only; the simulation ignores it.
    public sealed class TeamVision
    {
        private readonly List<(Vector3 position, float range)> _eyes = new List<(Vector3, float)>();
        private readonly HashSet<Unit> _visible = new HashSet<Unit>();

        public Team Team { get; private set; } = Team.None;
        public bool Enabled => Team == Team.Blue || Team == Team.Red;

        public void Update(Team team, IEnumerable<Unit> units)
        {
            Team = team;
            _eyes.Clear();
            _visible.Clear();
            if (!Enabled) return;
            foreach (Unit unit in units)
            {
                if (unit == null || unit.Object == null || unit.Definition == null || unit.Team != team || !unit.IsAlive) continue;
                float range = unit.Stats.Get(StatType.VisionRange);
                if (range > 0f) _eyes.Add((unit.Position, range));
            }
            foreach (Unit unit in units)
                if (unit != null && unit.Object != null && unit.Definition != null && Sees(unit)) _visible.Add(unit);
        }

        public bool IsVisible(Unit unit) => !Enabled || unit == null || _visible.Contains(unit);

        public bool IsPointVisible(Vector3 point)
        {
            if (!Enabled) return true;
            foreach ((Vector3 position, float range) in _eyes)
                if (UnitRegistry.FlatDistance(position, point) <= range) return true;
            return false;
        }

        private bool Sees(Unit unit)
        {
            if (unit.Team == Team || unit.IsStructure) return true;
            foreach ((Vector3 position, float range) in _eyes)
                if (UnitRegistry.FlatDistance(position, unit.Position) <= range + unit.Radius) return true;
            return false;
        }
    }
}
