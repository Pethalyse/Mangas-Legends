using System.Collections.Generic;
using MangaLegends.Core.Stats;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Services
{
    // What the local player's team sees this frame: a unit is visible if it is an ally, a structure, or stands inside
    // the vision radius of a living allied unit; a unit in a brush is only seen from inside the same brush (LoL rule);
    // stealthed wards are only seen by revealers (control wards), which also switch off enemy wards in their range.
    // Display only: the simulation ignores it.
    public sealed class TeamVision
    {
        private readonly List<(Vector3 position, float range, int brush)> _eyes = new List<(Vector3, float, int)>();
        private MapLayout _map;
        private readonly List<(Vector3 position, float range)> _revealers = new List<(Vector3, float)>();
        private readonly List<(Vector3 position, float range)> _enemyRevealers = new List<(Vector3, float)>();
        private readonly HashSet<Unit> _visible = new HashSet<Unit>();

        public Team Team { get; private set; } = Team.None;
        public bool Enabled => Team == Team.Blue || Team == Team.Red;

        public void Update(Team team, IEnumerable<Unit> units, MapLayout map)
        {
            Team = team;
            _map = map;
            _eyes.Clear();
            _visible.Clear();
            _revealers.Clear();
            _enemyRevealers.Clear();
            if (!Enabled) return;
            foreach (Unit unit in units)
            {
                if (unit == null || unit.Object == null || unit.Definition == null || !unit.IsAlive || !unit.Definition.RevealsStealth) continue;
                float range = unit.Stats.Get(StatType.VisionRange);
                if (unit.Team == team) _revealers.Add((unit.Position, range));
                else if (TeamRules.AreEnemies(unit.Team, team)) _enemyRevealers.Add((unit.Position, range));
            }
            foreach (Unit unit in units)
            {
                if (unit == null || unit.Object == null || unit.Definition == null || unit.Team != team || !unit.IsAlive) continue;
                if (unit.Definition.Stealthed && Within(_enemyRevealers, unit.Position, 0f)) continue;
                float range = unit.Stats.Get(StatType.VisionRange);
                if (range > 0f) _eyes.Add((unit.Position, range, BrushOf(unit.Position)));
            }
            foreach (Unit unit in units)
                if (unit != null && unit.Object != null && unit.Definition != null && Sees(unit)) _visible.Add(unit);
        }

        public bool IsVisible(Unit unit) => !Enabled || unit == null || _visible.Contains(unit);

        public bool IsPointVisible(Vector3 point)
        {
            if (!Enabled) return true;
            int brush = BrushOf(point);
            foreach ((Vector3 position, float range, int eyeBrush) in _eyes)
                if ((brush < 0 || eyeBrush == brush) && UnitRegistry.FlatDistance(position, point) <= range) return true;
            return false;
        }

        private static bool Within(List<(Vector3 position, float range)> sources, Vector3 point, float margin)
        {
            foreach ((Vector3 position, float range) in sources)
                if (UnitRegistry.FlatDistance(position, point) <= range + margin) return true;
            return false;
        }

        private int BrushOf(Vector3 position) => _map != null ? _map.BrushAt(position) : -1;

        private bool Sees(Unit unit)
        {
            if (unit.Team == Team || unit.IsStructure) return true;
            if (unit.Definition.Stealthed) return Within(_revealers, unit.Position, unit.Radius);
            int brush = BrushOf(unit.Position);
            foreach ((Vector3 position, float range, int eyeBrush) in _eyes)
                if ((brush < 0 || eyeBrush == brush) && UnitRegistry.FlatDistance(position, unit.Position) <= range + unit.Radius) return true;
            return false;
        }
    }
}
