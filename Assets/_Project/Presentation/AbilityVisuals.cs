using System.Collections.Generic;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Presentation
{
    // Draws the networked projectile, area and basic attack buffers with pooled primitives.
    public sealed class AbilityVisuals : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private MatchView _view;
        [SerializeField] private Material _material;
        [SerializeField] private Color _blue = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color _red = new Color(1f, 0.35f, 0.3f);
        [SerializeField] private Color _neutral = new Color(0.6f, 0.9f, 0.4f);
        [SerializeField] private float _areaAlpha = 0.35f;

        private readonly Pool _projectiles = new Pool(PrimitiveType.Sphere);
        private readonly Pool _areas = new Pool(PrimitiveType.Cylinder);
        private readonly Pool _shots = new Pool(PrimitiveType.Sphere);
        private readonly Pool _boxes = new Pool(PrimitiveType.Cube);
        private MaterialPropertyBlock _block;

        private void Awake() => _block = new MaterialPropertyBlock();

        private void LateUpdate()
        {
            MatchServices services = _view.Services;
            _projectiles.Begin();
            _areas.Begin();
            _shots.Begin();
            _boxes.Begin();
            if (services != null)
                foreach (Unit unit in services.Units.All)
                {
                    if (unit == null || unit.Object == null) continue;
                    Color color = unit.Team == Team.Blue ? _blue : unit.Team == Team.Red ? _red : _neutral;
                    if (unit.Caster != null)
                    {
                        DrawProjectiles(unit, color);
                        DrawAreas(unit, color);
                    }
                    if (unit.Attack != null && unit.Definition != null && unit.Definition.IsRanged) DrawShots(unit, color);
                }
            _projectiles.End();
            _areas.End();
            _shots.End();
            _boxes.End();
        }

        private void DrawProjectiles(Unit unit, Color color)
        {
            ProjectileLauncher launcher = unit.Caster.Projectiles;
            for (int i = 0; i < launcher.Projectiles.Length; i++)
            {
                NetProjectile projectile = launcher.Projectiles[i];
                if (!projectile.Active || !unit.Services.View.Vision.IsPointVisible(projectile.Position)) continue;
                ProjectileDefinition definition = launcher.DefinitionOf(projectile);
                float size = definition != null ? definition.RadiusAt(projectile.Rank) * 2f : 0.5f;
                Show(_projectiles.Next(_material, transform), projectile.Position, Vector3.one * size, color);
            }
        }

        private void DrawAreas(Unit unit, Color color)
        {
            AreaSpawner spawner = unit.Caster.Areas;
            for (int i = 0; i < spawner.Areas.Length; i++)
            {
                NetArea area = spawner.Areas[i];
                if (!area.Active || !unit.Services.View.Vision.IsPointVisible(area.Position)) continue;
                AreaDefinition definition = spawner.DefinitionOf(area);
                if (definition == null) continue;
                Color tinted = color;
                tinted.a = area.Activated ? _areaAlpha : _areaAlpha * 0.4f;
                if (definition.Shape == AreaShape.Rectangle)
                {
                    DrawRectangle(definition, area, tinted);
                    continue;
                }
                float diameter = definition.RadiusAt(area.Rank) * 2f;
                Show(_areas.Next(_material, transform), area.Position + Vector3.up * 0.05f, new Vector3(diameter, 0.02f, diameter), tinted);
                if (definition.IncomingOrb && !area.Activated) DrawOrb(unit, area, color);
            }
        }

        // A rectangle lies flat (laser, line), or stands up as a wall when the data gives it a height.
        private void DrawRectangle(AreaDefinition definition, in NetArea area, Color color)
        {
            float width = definition.WidthAt(area.Rank);
            float length = definition.LengthAt(area.Rank);
            float height = definition.WallHeight;
            Vector3 direction = area.Direction.sqrMagnitude > 0.0001f ? area.Direction : Vector3.forward;
            Transform box = _boxes.Next(_material, transform);
            box.rotation = Quaternion.LookRotation(direction, Vector3.up);
            if (height > 0f) color.a = Mathf.Max(color.a, 0.6f);
            Show(box, area.Position + direction * (length * 0.5f) + Vector3.up * (height > 0f ? height * 0.5f : 0.05f), new Vector3(width, height > 0f ? height : 0.02f, length), color);
        }

        // The orb rises from the caster and drops on the target point by the time the area lands.
        private void DrawOrb(Unit unit, in NetArea area, Color color)
        {
            float duration = Mathf.Max(1, area.ActivateTick - area.SpawnTick);
            float t = Mathf.Clamp01((unit.Runner.Tick - area.SpawnTick) / duration);
            Vector3 start = unit.Position + Vector3.up * 2.5f;
            Vector3 position = Vector3.Lerp(start, area.Position, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 4f;
            color.a = 1f;
            Show(_projectiles.Next(_material, transform), position, Vector3.one * 1.2f, color);
        }

        private void DrawShots(Unit unit, Color color)
        {
            AutoAttack attack = unit.Attack;
            int tick = unit.Runner.Tick;
            for (int i = 0; i < attack.Shots.Length; i++)
            {
                AttackShot shot = attack.Shots[i];
                if (!shot.Active || !unit.TryResolve(shot.Target, out Unit target)) continue;
                float duration = Mathf.Max(1, shot.HitTick - shot.FireTick);
                float t = Mathf.Clamp01((tick - shot.FireTick) / duration);
                Vector3 position = Vector3.Lerp(unit.Position + Vector3.up, target.Position + Vector3.up, t);
                if (!unit.Services.View.Vision.IsPointVisible(position)) continue;
                Show(_shots.Next(_material, transform), position, Vector3.one * 0.3f, color);
            }
        }

        private void Show(Transform item, Vector3 position, Vector3 scale, Color color)
        {
            item.position = position;
            item.localScale = scale;
            Renderer renderer = item.GetComponent<Renderer>();
            renderer.GetPropertyBlock(_block);
            _block.SetColor(ColorId, color);
            _block.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(_block);
        }

        private sealed class Pool
        {
            private readonly PrimitiveType _type;
            private readonly List<Transform> _items = new List<Transform>();
            private int _used;

            public Pool(PrimitiveType type) => _type = type;

            public void Begin() => _used = 0;

            public Transform Next(Material material, Transform parent)
            {
                if (_used == _items.Count)
                {
                    GameObject created = GameObject.CreatePrimitive(_type);
                    Object.Destroy(created.GetComponent<Collider>());
                    created.transform.SetParent(parent, false);
                    if (material != null) created.GetComponent<Renderer>().sharedMaterial = material;
                    _items.Add(created.transform);
                }
                Transform item = _items[_used++];
                item.gameObject.SetActive(true);
                return item;
            }

            public void End()
            {
                for (int i = _used; i < _items.Count; i++) _items[i].gameObject.SetActive(false);
            }
        }
    }
}
