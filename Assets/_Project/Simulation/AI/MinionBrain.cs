using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.AI
{
    [RequireComponent(typeof(Unit))]
    public sealed class MinionBrain : NetworkBehaviour, IUnitBrain
    {
        private const int RetargetPeriod = 6;
        private const float WaypointReached = 2f;
        private const float LeashBonus = 3f;
        private const float FollowDistance = 3f;

        private Unit _unit;

        [Networked] public byte LaneIndex { get; set; }
        [Networked] public byte NextWaypoint { get; set; }
        [Networked] public NetworkId Owner { get; set; }
        [Networked] public int ExpireTick { get; set; }

        private void Awake() => _unit = GetComponent<Unit>();

        public void AssignLane(int lane)
        {
            LaneIndex = (byte)lane;
            NextWaypoint = 1;
        }

        public void MakeSummon(Unit owner, float durationSeconds)
        {
            Owner = owner.Object.Id;
            LaneIndex = MatchController.NoLane;
            ExpireTick = durationSeconds > 0f ? Runner.Tick + TickTime.SecondsToTicks(durationSeconds, Runner.TickRate) : 0;
        }

        public void Think()
        {
            if (!HasStateAuthority) return;
            if (ExpireTick > 0 && Runner.Tick >= ExpireTick)
            {
                Runner.Despawn(Object);
                return;
            }

            float aggro = _unit.Services.Rules.MinionAggroRange;
            if (_unit.TryResolve(_unit.Attack.Target, out Unit current) && !TargetSelection.ShouldDrop(_unit, current, aggro + LeashBonus)) return;
            if (current != null || TargetSelection.IsRetargetTick(_unit, RetargetPeriod))
            {
                Unit target = TargetSelection.Pick(_unit, aggro);
                if (target != null)
                {
                    _unit.Attack.SetTarget(target);
                    return;
                }
                _unit.Attack.ClearTarget();
            }
            if (_unit.Attack.HasTarget) return;
            if (LaneIndex == MatchController.NoLane) FollowOwner();
            else FollowLane();
        }

        private void FollowLane()
        {
            MapLayout map = _unit.Services.Map;
            if (map == null || LaneIndex >= map.Lanes.Count) return;
            LanePath lane = map.Lanes[LaneIndex];
            Vector3 waypoint = lane.WaypointFor(_unit.Team, NextWaypoint);
            if (UnitRegistry.FlatDistance(_unit.Position, waypoint) <= WaypointReached && NextWaypoint < lane.Waypoints.Count - 1)
            {
                NextWaypoint++;
                waypoint = lane.WaypointFor(_unit.Team, NextWaypoint);
            }
            if (!_unit.Motor.IsMoving || UnitRegistry.FlatDistance(_unit.Motor.Destination, waypoint) > WaypointReached)
                _unit.Motor.MoveTo(waypoint);
        }

        private void FollowOwner()
        {
            if (!_unit.TryResolve(Owner, out Unit owner) || !owner.IsAlive)
            {
                _unit.Motor.Stop();
                return;
            }
            if (UnitRegistry.FlatDistance(_unit.Position, owner.Position) > FollowDistance && !_unit.Motor.IsMoving)
                _unit.Motor.MoveTo(owner.Position, FollowDistance * 0.5f);
        }
    }
}
