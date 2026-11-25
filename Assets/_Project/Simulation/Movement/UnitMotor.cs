using Fusion;
using MangaLegends.Core.Stats;
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
            if (!IsMoving || !_unit.Status.CanMove) return;
            Advance(Speed * Runner.DeltaTime);
        }

        public bool MoveTo(Vector3 point, float stopDistance = 0f)
        {
            if (IsDashing) return false;
            Vector3 start = transform.position;
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas)) return false;
            if (!NavMesh.CalculatePath(start, hit.position, NavMesh.AllAreas, _path) || _path.corners.Length == 0)
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
            if (!passesThroughWalls && NavMesh.Raycast(start, end, out NavMeshHit wall, NavMesh.AllAreas)) target = wall.position;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas)) target = hit.position;
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
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas)) position = hit.position;
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
            Place(position);
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
                Place(DashEnd);
                IsDashing = false;
                return;
            }
            transform.rotation = Quaternion.LookRotation(toEnd);
            Place(position + toEnd.normalized * step);
        }

        private void Place(Vector3 position)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) position.y = hit.position.y;
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
