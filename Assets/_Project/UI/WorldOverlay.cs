using System.Collections.Generic;
using MangaLegends.Core.Combat;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    public sealed class WorldOverlay
    {
        private const float FloatSeconds = 1f;

        private readonly Transform _root;
        private readonly List<(Image back, Image fill)> _bars = new List<(Image, Image)>();
        private readonly List<(Text text, Vector3 world, float age)> _floating = new List<(Text, Vector3, float)>();
        private readonly Stack<Text> _freeTexts = new Stack<Text>();
        private readonly Dictionary<Unit, int> _seenHits = new Dictionary<Unit, int>();

        public WorldOverlay(Transform canvas)
        {
            _root = UiFactory.Rect(canvas, "World", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _root.SetAsFirstSibling();
        }

        public void Refresh(MatchServices services)
        {
            Camera camera = services?.View.Camera;
            int used = 0;
            if (camera != null)
                foreach (Unit unit in services.Units.All)
                {
                    if (unit == null || unit.Object == null || unit.Definition == null) continue;
                    CollectHits(unit);
                    if (!unit.IsAlive) continue;
                    Vector3 screen = camera.WorldToScreenPoint(unit.Position + Vector3.up * (unit.IsStructure ? 4f : 2.4f));
                    if (screen.z <= 0f) continue;
                    DrawBar(used++, unit, screen, services.View.LocalChampion);
                }
            for (int i = used; i < _bars.Count; i++) _bars[i].back.gameObject.SetActive(false);
            Animate(camera);
        }

        private void DrawBar(int index, Unit unit, Vector3 screen, Unit local)
        {
            if (index == _bars.Count)
            {
                Image back = UiFactory.Bar(_root, "Bar", Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, Vector2.one, Color.white, out Image created);
                _bars.Add((back, created));
            }
            var bar = _bars[index];
            bar.back.gameObject.SetActive(true);
            float width = unit.IsChampion ? 110f : unit.IsStructure ? 160f : 60f;
            RectTransform rect = bar.back.rectTransform;
            rect.position = screen;
            rect.sizeDelta = new Vector2(width, unit.IsChampion ? 12f : 7f);
            bool ally = local != null ? unit.Team == local.Team : unit.Team == Team.Blue;
            bar.fill.color = unit.Team == Team.Neutral ? new Color(0.9f, 0.8f, 0.3f) : unit == local ? UiFactory.Health : ally ? new Color(0.3f, 0.6f, 1f) : new Color(0.95f, 0.3f, 0.3f);
            UiFactory.SetFill(bar.fill, unit.Health.Ratio);
        }

        private void CollectHits(Unit unit)
        {
            Health health = unit.Health;
            int count = health.HitCount;
            if (!_seenHits.TryGetValue(unit, out int seen))
            {
                _seenHits[unit] = count;
                return;
            }
            if (count - seen > Health.HitHistory) seen = count - Health.HitHistory;
            for (int sequence = seen; sequence < count; sequence++) Spawn(unit, health.HitAt(sequence));
            _seenHits[unit] = count;
        }

        private void Spawn(Unit unit, HitRecord hit)
        {
            if (hit.Amount < 1f) return;
            Text text = _freeTexts.Count > 0 ? _freeTexts.Pop() : UiFactory.Text(_root, "Float", "", 26, TextAnchor.MiddleCenter, Color.white);
            text.gameObject.SetActive(true);
            text.rectTransform.sizeDelta = new Vector2(160f, 40f);
            bool critical = (hit.Flags & (byte)DamageFlags.Critical) != 0;
            text.text = (hit.IsHeal ? "+" : "") + Mathf.RoundToInt(hit.Amount) + (critical ? " !" : "");
            DamageType type = (DamageType)hit.Type;
            text.color = hit.IsHeal ? new Color(0.4f, 1f, 0.4f) : type == DamageType.Magical ? new Color(0.6f, 0.6f, 1f) : type == DamageType.True ? Color.white : new Color(1f, 0.6f, 0.3f);
            text.fontSize = critical ? 34 : 26;
            Vector3 jitter = new Vector3((_floating.Count % 5 - 2) * 0.3f, 0f, 0f);
            _floating.Add((text, unit.Position + Vector3.up * 2.8f + jitter, 0f));
        }

        private void Animate(Camera camera)
        {
            for (int i = _floating.Count - 1; i >= 0; i--)
            {
                var entry = _floating[i];
                entry.age += Time.deltaTime;
                if (entry.age >= FloatSeconds || camera == null)
                {
                    entry.text.gameObject.SetActive(false);
                    _freeTexts.Push(entry.text);
                    _floating.RemoveAt(i);
                    continue;
                }
                Vector3 screen = camera.WorldToScreenPoint(entry.world + Vector3.up * entry.age * 1.5f);
                entry.text.rectTransform.position = screen;
                Color color = entry.text.color;
                color.a = 1f - entry.age / FloatSeconds;
                entry.text.color = color;
                _floating[i] = entry;
            }
        }
    }
}
