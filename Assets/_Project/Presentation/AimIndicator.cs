using System.Collections.Generic;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Input;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Presentation
{
    // Ground indicators of the LoL client: range ring, then the spell shape towards the cursor while a cast key aims.
    public sealed class AimIndicator : MonoBehaviour
    {
        private const float Height = 0.06f;
        private const int CircleSegments = 64;

        [SerializeField] private MatchView _view;
        [SerializeField] private Material _material;
        [SerializeField] private Color _rangeColor = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] private Color _shapeColor = new Color(0.45f, 0.8f, 1f, 0.28f);
        [SerializeField] private Color _edgeColor = new Color(0.6f, 0.9f, 1f, 0.8f);
        [SerializeField] private Color _unavailableColor = new Color(1f, 0.35f, 0.3f, 0.3f);
        [SerializeField] private Color _targetColor = new Color(1f, 0.85f, 0.3f, 0.9f);
        [SerializeField] private float _lineThickness = 0.08f;
        [SerializeField] private float _maxRangeRing = 50f;

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly List<int> _triangles = new List<int>();
        private Mesh _mesh;

        private void Awake()
        {
            _mesh = new Mesh { name = "AimIndicator" };
            _mesh.MarkDynamic();
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }

        private void LateUpdate()
        {
            _vertices.Clear();
            _colors.Clear();
            _triangles.Clear();
            MatchServices services = _view.Services;
            if (services != null && services.View.IsAiming) Build(services.View);
            else if (services != null) BuildHoverRange(services.View);
            if (_vertices.Count == 0) return;

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetTriangles(_triangles, 0);
            Graphics.DrawMesh(_mesh, Matrix4x4.identity, _material, 0);
        }

        private void Build(LocalView view)
        {
            Unit champion = view.LocalChampion;
            if (champion == null || champion.Object == null || !champion.IsAlive) return;
            Vector3 origin = champion.Position;
            origin.y = Height;
            Vector3 cursor = view.PointerWorld;
            cursor.y = Height;

            if (view.AimButton == InputButton.AttackMove)
            {
                AddRing(origin, champion.Stats.Get(StatType.AttackRange) + champion.Radius, _rangeColor);
                return;
            }

            AimSource source = AimSource.For(champion, view.AimButton);
            if (!source.IsValid) return;
            AimShape shape = AimShape.Describe(source, champion.Radius);
            Color fill = source.Ready ? _shapeColor : _unavailableColor;
            Color edge = source.Ready ? _edgeColor : _unavailableColor;
            if (shape.Range > 0f && shape.Range <= _maxRangeRing) AddRing(origin, shape.Range + (source.Targeting.Mode == TargetingMode.Unit ? champion.Radius : 0f), _rangeColor);

            Vector3 toCursor = cursor - origin;
            Vector3 direction = toCursor.sqrMagnitude > 0.0001f ? toCursor.normalized : champion.transform.forward;
            switch (shape.Kind)
            {
                case AimShapeKind.Line:
                    for (int i = 0; i < Mathf.Max(1, shape.Count); i++)
                    {
                        Vector3 lineDirection = Quaternion.Euler(0f, SpreadOffset(i, shape.Count, shape.Spread), 0f) * direction;
                        AddRect(origin, lineDirection, shape.Length, shape.Width, fill, edge);
                    }
                    break;
                case AimShapeKind.Cone:
                    AddCone(origin, direction, shape.Radius, shape.Angle, fill, edge);
                    break;
                case AimShapeKind.Circle:
                    Vector3 center = origin;
                    if (shape.AtCursor) center = !source.Targeting.WalkIntoRange && shape.Range > 0f && shape.Range <= _maxRangeRing ? origin + Vector3.ClampMagnitude(toCursor, shape.Range) : cursor;
                    AddDisc(center, shape.Radius, fill);
                    AddRing(center, shape.Radius, edge);
                    if (shape.InnerRadius > 0f && shape.InnerRadius < shape.Radius) AddRing(center, shape.InnerRadius, edge);
                    break;
            }

            Unit hovered = view.AimTarget != null ? view.AimTarget : view.HoveredUnit;
            if (source.Targeting.Mode == TargetingMode.Unit && hovered != null && hovered.IsAlive && TeamRules.Matches(source.Targeting.ValidTargets, champion, hovered))
            {
                Vector3 at = hovered.Position;
                at.y = Height;
                AddRing(at, hovered.Radius + 0.25f, _targetColor);
            }
        }

        // Hovering a spell or an item in the HUD shows its range around the champion.
        private void BuildHoverRange(LocalView view)
        {
            Unit champion = view.LocalChampion;
            if (view.HoverRange <= 0f || champion == null || champion.Object == null || !champion.IsAlive) return;
            Vector3 origin = champion.Position;
            origin.y = Height;
            AddRing(origin, view.HoverRange + champion.Radius, _rangeColor);
        }

        private static float SpreadOffset(int index, int count, float spread) =>
            count <= 1 ? 0f : -spread * 0.5f + spread * index / (count - 1);

        private void AddRing(Vector3 center, float radius, Color color)
        {
            float inner = Mathf.Max(0f, radius - _lineThickness * 0.5f);
            float outer = radius + _lineThickness * 0.5f;
            int start = _vertices.Count;
            for (int i = 0; i <= CircleSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / CircleSegments;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                AddVertex(center + offset * inner, color);
                AddVertex(center + offset * outer, color);
            }
            for (int i = 0; i < CircleSegments; i++) AddQuad(start + i * 2, start + i * 2 + 1, start + i * 2 + 3, start + i * 2 + 2);
        }

        private void AddDisc(Vector3 center, float radius, Color color)
        {
            int start = _vertices.Count;
            AddVertex(center, color);
            for (int i = 0; i <= CircleSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / CircleSegments;
                AddVertex(center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius, color);
            }
            for (int i = 0; i < CircleSegments; i++) AddTriangle(start, start + i + 2, start + i + 1);
        }

        private void AddRect(Vector3 origin, Vector3 direction, float length, float width, Color fill, Color edge)
        {
            Vector3 side = Vector3.Cross(Vector3.up, direction) * (width * 0.5f);
            Vector3 end = origin + direction * length;
            int start = _vertices.Count;
            AddVertex(origin - side, fill);
            AddVertex(origin + side, fill);
            AddVertex(end + side, fill);
            AddVertex(end - side, fill);
            AddQuad(start, start + 3, start + 2, start + 1);
            AddSegment(origin - side, end - side, edge);
            AddSegment(origin + side, end + side, edge);
            AddSegment(end - side, end + side, edge);
        }

        private void AddCone(Vector3 origin, Vector3 direction, float radius, float angle, Color fill, Color edge)
        {
            const int segments = 24;
            int start = _vertices.Count;
            AddVertex(origin, fill);
            for (int i = 0; i <= segments; i++)
            {
                float offset = -angle * 0.5f + angle * i / segments;
                AddVertex(origin + Quaternion.Euler(0f, offset, 0f) * direction * radius, fill);
            }
            for (int i = 0; i < segments; i++) AddTriangle(start, start + i + 1, start + i + 2);
            Vector3 left = origin + Quaternion.Euler(0f, -angle * 0.5f, 0f) * direction * radius;
            Vector3 right = origin + Quaternion.Euler(0f, angle * 0.5f, 0f) * direction * radius;
            AddSegment(origin, left, edge);
            AddSegment(origin, right, edge);
            Vector3 previous = left;
            for (int i = 1; i <= segments; i++)
            {
                Vector3 next = origin + Quaternion.Euler(0f, -angle * 0.5f + angle * i / segments, 0f) * direction * radius;
                AddSegment(previous, next, edge);
                previous = next;
            }
        }

        private void AddSegment(Vector3 from, Vector3 to, Color color)
        {
            Vector3 along = to - from;
            if (along.sqrMagnitude < 0.000001f) return;
            Vector3 side = Vector3.Cross(Vector3.up, along.normalized) * (_lineThickness * 0.5f);
            int start = _vertices.Count;
            AddVertex(from - side, color);
            AddVertex(from + side, color);
            AddVertex(to + side, color);
            AddVertex(to - side, color);
            AddQuad(start, start + 3, start + 2, start + 1);
        }

        private void AddVertex(Vector3 position, Color color)
        {
            _vertices.Add(position);
            _colors.Add(color);
        }

        private void AddTriangle(int a, int b, int c)
        {
            _triangles.Add(a);
            _triangles.Add(b);
            _triangles.Add(c);
        }

        private void AddQuad(int a, int b, int c, int d)
        {
            AddTriangle(a, b, c);
            AddTriangle(a, c, d);
        }
    }
}
