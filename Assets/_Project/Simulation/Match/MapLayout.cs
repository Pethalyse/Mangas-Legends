using System;
using System.Collections.Generic;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Match
{
    [Serializable]
    public sealed class LanePath
    {
        [SerializeField] private string _name;
        [SerializeField] private List<Transform> _waypointsFromBlue = new List<Transform>();

        public string Name => _name;
        public IReadOnlyList<Transform> Waypoints => _waypointsFromBlue;

        public Vector3 WaypointFor(Team team, int index)
        {
            int count = _waypointsFromBlue.Count;
            int clamped = Mathf.Clamp(index, 0, count - 1);
            return _waypointsFromBlue[team == Team.Red ? count - 1 - clamped : clamped].position;
        }
    }

    [Serializable]
    public sealed class StructurePlacement
    {
        [SerializeField] private UnitDefinition _definition;
        [SerializeField] private Team _team;
        [SerializeField] private int _lane = -1;
        [SerializeField] private Transform _point;
        [SerializeField] private ProtectionMode _protection;
        [SerializeField] private List<int> _protectors = new List<int>();

        public UnitDefinition Definition => _definition;
        public Team Team => _team;
        public int Lane => _lane;
        public Transform Point => _point;
        public ProtectionMode Protection => _protection;
        public IReadOnlyList<int> Protectors => _protectors;
    }

    [Serializable]
    public sealed class CampPlacement
    {
        [SerializeField] private string _name;
        [SerializeField] private List<UnitDefinition> _monsters = new List<UnitDefinition>();
        [SerializeField] private Transform _point;
        [SerializeField, Min(0f)] private float _firstSpawnSeconds = 55f;
        [SerializeField, Min(0f)] private float _respawnSeconds = 135f;
        [SerializeField, Min(0f)] private float _leashRadius = 8f;

        public string Name => _name;
        public IReadOnlyList<UnitDefinition> Monsters => _monsters;
        public Transform Point => _point;
        public float FirstSpawnSeconds => _firstSpawnSeconds;
        public float RespawnSeconds => _respawnSeconds;
        public float LeashRadius => _leashRadius;
    }

    public enum ZoneShape
    {
        Rectangle,
        Circle
    }

    // A flat area of the map: a rectangle oriented by its point's yaw, or a circle (size.x = diameter).
    [Serializable]
    public sealed class MapZone
    {
        [SerializeField] private string _name;
        [SerializeField] private Transform _point;
        [SerializeField] private ZoneShape _shape;
        [SerializeField] private Vector2 _size = new Vector2(4f, 4f);

        public string Name => _name;
        public Transform Point => _point;
        public ZoneShape Shape => _shape;
        public Vector2 Size => _size;

        public bool Contains(Vector3 position)
        {
            if (_point == null) return false;
            Vector3 offset = position - _point.position;
            offset.y = 0f;
            if (_shape == ZoneShape.Circle) return offset.sqrMagnitude <= _size.x * _size.x * 0.25f;
            Vector3 local = Quaternion.Inverse(_point.rotation) * offset;
            return Mathf.Abs(local.x) <= _size.x * 0.5f && Mathf.Abs(local.z) <= _size.y * 0.5f;
        }
    }

    // Level design of one map: everything here is placement, the rules stay in GameRulesConfig.
    public sealed class MapLayout : MonoBehaviour
    {
        [SerializeField] private Transform _blueFountain;
        [SerializeField] private Transform _redFountain;
        [SerializeField] private List<LanePath> _lanes = new List<LanePath>();
        [SerializeField] private List<StructurePlacement> _structures = new List<StructurePlacement>();
        [SerializeField] private List<CampPlacement> _camps = new List<CampPlacement>();
        [SerializeField] private List<MapZone> _river = new List<MapZone>();
        [SerializeField] private List<MapZone> _brushes = new List<MapZone>();
        [SerializeField] private Vector2 _size = new Vector2(160f, 160f);

        public IReadOnlyList<LanePath> Lanes => _lanes;
        public IReadOnlyList<StructurePlacement> Structures => _structures;
        public IReadOnlyList<CampPlacement> Camps => _camps;
        public IReadOnlyList<MapZone> River => _river;
        public IReadOnlyList<MapZone> Brushes => _brushes;
        public Vector2 Size => _size;

        public bool IsInRiver(Vector3 position)
        {
            foreach (MapZone zone in _river)
                if (zone.Contains(position)) return true;
            return false;
        }

        public int BrushAt(Vector3 position)
        {
            for (int i = 0; i < _brushes.Count; i++)
                if (_brushes[i].Contains(position)) return i;
            return -1;
        }

        public Vector3 Fountain(Team team) => (team == Team.Red ? _redFountain : _blueFountain).position;
    }
}
