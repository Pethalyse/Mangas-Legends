using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.AI;

namespace MangaLegends.Simulation.Movement
{
    // The path lives in networked state so that a resimulated tick follows exactly the same corners.
    public sealed class UnitMotor : NetworkBehaviour
    {
        private const int MaxCorners = 16;
        private const float CornerReachedDistance = 0.05f;
        private const float NavMeshSampleDistance = 4f;
        // A dash through a wall that ends inside it lands on the closest walkable point instead (like Kayn's E in LoL).
        private const float WallExitSearchDistance = 10f;
        private const float SeparationShare = 0.25f;
        private const float SeparationMaxStepRatio = 1.5f;
        private const float RepathSeconds = 0.25f;
        private const float RepathTolerance = 1.5f;
        private const int SeparationPeriod = 2;

        private readonly List<Unit> _neighbours = new List<Unit>();
        private const int LaneArea = 3;
        // Structures sit on a NavMesh area of their own: solid for champions and monsters, open to minions so waves walk straight down the lane.
        private const int StructureArea = 4;
        private const float OffLaneCostForMinions = 4f;
        private NavMeshQueryFilter _minionFilter;
        private bool _hasMinionFilter;

        private NavMeshPath _path;
        private Unit _unit;
        private NetworkTransform _networkTransform;

        [Networked] public NetworkBool IsMoving { get; set; }
        [Networked] public Vector3 Destination { get; set; }
        [Networked] public float StopDistance { get; set; }
        [Networked] public byte CornerIndex { get; set; }
        [Networked] public byte CornerCount { get; set; }
        [Networked, Capacity(MaxCorners)] public NetworkArray<Vector3> Corners => default;
        [Networked] public NetworkBool IsDashing { get; set; }
        [Networked] public Vector3 DashEnd { get; set; }
        [Networked] public float DashSpeed { get; set; }
        [Networked] public int NextPathTick { get; set; }
        [Networked] public Vector3 RequestedPoint { get; set; }

        public bool IsForcedMovement => IsDashing;
        public float Speed => _unit != null ? _unit.Stats.Get(StatType.MoveSpeed) : 0f;

        private void Awake()
        {
            _networkTransform = GetComponent<NetworkTransform>();
            _path = new NavMeshPath();
        }

        public void Initialize(Unit unit) => _unit = unit;

        public void Simulate()
        {
            if (!_unit.IsAlive)
            {
                IsMoving = false;
                IsDashing = false;
                return;
            }
            if (IsDashing)
            {
                SimulateDash();
                return;
            }
            if (IsMoving && _unit.Status.CanMove) Advance(Speed * Runner.DeltaTime);
            if (HasStateAuthority && (_unit.Kind == UnitKind.Minion || _unit.IsMonster) && (Runner.Tick + _unit.SortKey) % SeparationPeriod == 0) Separate();
        }

        // Host-only soft collision for units that are never predicted: minions and monsters stop stacking on one spot.
        private void Separate()
        {
            Vector3 position = transform.position;
            float radius = _unit.Radius;
            _unit.Services.Units.Nearby(position, radius * 2f + 1f, Runner.Tick, _neighbours);
            Vector3 push = Vector3.zero;
            foreach (Unit other in _neighbours)
            {
                if (other == _unit || !other.IsAlive || other.Kind == UnitKind.Ward) continue;
                Vector3 away = position - other.Position;
                away.y = 0f;
                float distance = away.magnitude;
                float overlap = radius + other.Radius - distance;
                if (overlap <= 0f) continue;
                // LoL body-blocking is very light: minions walk through champions and only nudge each other.
                if (other.IsChampion) continue;
                if (other.IsStructure && _unit.Kind == UnitKind.Minion) continue;
                float share = other.IsStructure ? 1f : SeparationShare;
                Vector3 direction = distance > 0.0001f ? away / distance : Quaternion.Euler(0f, (_unit.SortKey % 360u), 0f) * Vector3.forward;
                push += direction * (overlap * share);
            }
            if (push.sqrMagnitude < 0.000001f) return;
            push = Vector3.ClampMagnitude(push, Mathf.Max(0.05f, Speed) * Runner.DeltaTime * SeparationMaxStepRatio * SeparationPeriod);
            Vector3 target = position + push;
            if (NavMesh.Raycast(position, target, out NavMeshHit wall, WalkMask)) target = wall.position;
            Place(target, false);
        }

        private int WalkMask => _unit.Kind == UnitKind.Minion ? NavMesh.AllAreas : NavMesh.AllAreas & ~(1 << StructureArea);

        public bool MoveTo(Vector3 point, float stopDistance = 0f)
        {
            if (IsDashing) return false;
            // AI units ask for paths every tick; recomputing the same one is what made large waves lag.
            if (!_unit.IsChampion && Runner.Tick < NextPathTick && IsMoving && FlatDistance(point, RequestedPoint) <= RepathTolerance) return true;
            NextPathTick = Runner.Tick + Mathf.Max(1, Core.Simulation.TickTime.SecondsToTicks(RepathSeconds, Runner.TickRate));
            RequestedPoint = point;
            Vector3 start = transform.position;
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, NavMeshSampleDistance, WalkMask)) return false;
            if (!CalculatePath(start, hit.position) || _path.corners.Length == 0)
            {
                Stop();
                return false;
            }

            Vector3[] corners = _path.corners;
            int count = Mathf.Min(corners.Length, MaxCorners);
            for (int i = 0; i < count; i++) Corners.Set(i, corners[i]);
            CornerCount = (byte)count;
            CornerIndex = (byte)(count > 1 ? 1 : 0);
            Destination = corners[count - 1];
            StopDistance = Mathf.Max(0f, stopDistance);
            IsMoving = true;
            return true;
        }

        // Minions price everything that is not a lane higher, so they only leave the lane when it is blocked.
        private bool CalculatePath(Vector3 start, Vector3 end)
        {
            if (_unit.Kind != UnitKind.Minion) return NavMesh.CalculatePath(start, end, WalkMask, _path);
            if (!_hasMinionFilter)
            {
                _minionFilter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, agentTypeID = 0 };
                for (int area = 0; area < 32; area++) _minionFilter.SetAreaCost(area, area == LaneArea || area == StructureArea ? 1f : OffLaneCostForMinions);
                _hasMinionFilter = true;
            }
            return NavMesh.CalculatePath(start, end, _minionFilter, _path);
        }

        public void Stop()
        {
            IsMoving = false;
            CornerCount = 0;
            CornerIndex = 0;
        }

        public void StartDash(Vector3 end, float speed, bool passesThroughWalls)
        {
            Vector3 start = transform.position;
            Vector3 target = end;
            if (!passesThroughWalls && NavMesh.Raycast(start, end, out NavMeshHit wall, WalkMask)) target = wall.position;
            float search = passesThroughWalls ? WallExitSearchDistance : NavMeshSampleDistance;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, search, WalkMask)) target = hit.position;
            else target = start;

            Stop();
            if (speed <= 0f)
            {
                Place(target);
                return;
            }
            DashEnd = target;
            DashSpeed = speed;
            IsDashing = true;
        }

        public void Teleport(Vector3 position)
        {
            Stop();
            IsDashing = false;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, NavMeshSampleDistance, WalkMask)) position = hit.position;
            if (_networkTransform != null) _networkTransform.Teleport(position, transform.rotation);
            else transform.position = position;
        }

        public void Face(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        private void Advance(float budget)
        {
            Vector3 position = transform.position;
            while (budget > 0f && CornerIndex < CornerCount)
            {
                Vector3 corner = Corners[CornerIndex];
                float remainingToEnd = RemainingDistance(position);
                if (remainingToEnd <= StopDistance)
                {
                    Stop();
                    break;
                }
                Vector3 toCorner = corner - position;
                toCorner.y = 0f;
                float distance = toCorner.magnitude;
                float step = Mathf.Min(budget, distance, Mathf.Max(0f, remainingToEnd - StopDistance));
                if (distance > 0.0001f)
                {
                    position += toCorner / distance * step;
                    transform.rotation = Quaternion.LookRotation(toCorner);
                }
                budget -= step;
                if (distance - step <= CornerReachedDistance) CornerIndex++;
                if (step <= 0f) break;
            }
            if (CornerIndex >= CornerCount) Stop();
            Place(position, false);
        }

        private float RemainingDistance(Vector3 position)
        {
            float total = 0f;
            Vector3 from = position;
            for (int i = CornerIndex; i < CornerCount; i++)
            {
                Vector3 corner = Corners[i];
                total += FlatDistance(from, corner);
                from = corner;
            }
            return total;
        }

        private void SimulateDash()
        {
            Vector3 position = transform.position;
            Vector3 toEnd = DashEnd - position;
            toEnd.y = 0f;
            float step = DashSpeed * Runner.DeltaTime;
            if (toEnd.magnitude <= step)
            {
                Place(DashEnd, false);
                IsDashing = false;
                return;
            }
            transform.rotation = Quaternion.LookRotation(toEnd);
            Place(position + toEnd.normalized * step, false);
        }

        private void Place(Vector3 position, bool sampleHeight = true)
        {
            if (sampleHeight && NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) position.y = hit.position.y;
            transform.position = position;
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
