using System.Collections.Generic;
using MangaLegends.Core.Combat;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // Health bar above a unit, LoL-like: champions show their level, name and mana; flat on the screen.
    internal sealed class BarView
    {
        private readonly RectTransform _root;
        private readonly Image _fill;
        private readonly Image _shield;
        private readonly Image _magicShield;
        private readonly Image _physicalShield;
        private readonly Image _mana;
        private readonly GameObject _manaBack;
        private readonly GameObject _levelBox;
        private readonly Text _level;
        private readonly Text _name;

        public BarView(Transform parent)
        {
            Image back = UiFactory.Box(parent, "Bar", Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, Vector2.one, new Color(0.05f, 0.05f, 0.07f, 0.9f));
            back.raycastTarget = false;
            _root = back.rectTransform;
            _fill = UiFactory.Image(_root, "Fill", Color.white);
            _fill.rectTransform.pivot = new Vector2(0f, 0.5f);
            _shield = UiFactory.Image(_root, "Shield", new Color(1f, 1f, 1f, 0.85f));
            _physicalShield = UiFactory.Image(_root, "ShieldPhysical", new Color(1f, 0.6f, 0.25f, 0.9f));
            _magicShield = UiFactory.Image(_root, "ShieldMagic", new Color(0.65f, 0.35f, 1f, 0.9f));
            Image manaBack = UiFactory.Box(_root, "ManaBack", new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.one, new Color(0.05f, 0.05f, 0.07f, 0.9f));
            _manaBack = manaBack.gameObject;
            _mana = UiFactory.Image(manaBack.transform, "Mana", UiFactory.Mana);
            Image level = UiFactory.Box(_root, "Level", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-2f, -3f), new Vector2(26f, 26f), new Color(0.05f, 0.05f, 0.07f, 0.95f));
            _levelBox = level.gameObject;
            _level = UiFactory.Text(level.transform, "Text", "", 16, TextAnchor.MiddleCenter, Color.white);
            _name = UiFactory.Text(_root, "Name", "", 15, TextAnchor.LowerCenter, Color.white);
            _name.rectTransform.anchorMin = new Vector2(0f, 1f);
            _name.rectTransform.anchorMax = new Vector2(1f, 1f);
            _name.rectTransform.offsetMin = new Vector2(-40f, 2f);
            _name.rectTransform.offsetMax = new Vector2(40f, 22f);
            _name.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);
        }

        public void Show(Unit unit, Vector3 screen, Color color)
        {
            _root.gameObject.SetActive(true);
            bool champion = unit.IsChampion;
            Vector2 size = champion ? new Vector2(150f, 18f) : unit.IsStructure ? new Vector2(220f, 16f) : unit.IsMonster ? new Vector2(120f, 12f) : new Vector2(70f, 8f);
            _root.position = screen;
            _root.sizeDelta = size;
            float max = unit.Health.Max;
            float shield = unit.Health.ShieldTotal;
            float total = Mathf.Max(max, unit.Health.Current + shield);
            _fill.color = color;
            Span(_fill.rectTransform, 0f, unit.Health.Current / total);
            float any = unit.Health.ShieldOf(Core.Combat.ShieldFilter.Any);
            float physical = unit.Health.ShieldOf(Core.Combat.ShieldFilter.Physical);
            float magic = unit.Health.ShieldOf(Core.Combat.ShieldFilter.Magical);
            float from = unit.Health.Current;
            Segment(_shield, ref from, any, total);
            Segment(_physicalShield, ref from, physical, total);
            Segment(_magicShield, ref from, magic, total);
            bool mana = champion && unit.Resource != null && unit.Resource.UsesResource;
            _manaBack.SetActive(mana);
            if (mana)
            {
                RectTransform back = (RectTransform)_manaBack.transform;
                back.anchorMin = new Vector2(0f, 0f);
                back.anchorMax = new Vector2(1f, 0f);
                back.offsetMin = new Vector2(0f, -6f);
                back.offsetMax = new Vector2(0f, 0f);
                Span(_mana.rectTransform, 0f, unit.Resource.Ratio);
            }
            _levelBox.SetActive(champion);
            _name.gameObject.SetActive(champion);
            if (!champion) return;
            _level.text = unit.Stats.Level.ToString();
            string player = unit.GetComponent<Simulation.Champions.ChampionProgress>().PlayerName.ToString();
            _name.text = string.IsNullOrEmpty(player) ? unit.Definition.DisplayName : player;
        }

        private static void Segment(Image image, ref float from, float amount, float total)
        {
            image.gameObject.SetActive(amount > 0f);
            if (amount <= 0f) return;
            Span(image.rectTransform, from / total, (from + amount) / total);
            from += amount;
        }

        public void Hide() => _root.gameObject.SetActive(false);

        private static void Span(RectTransform rect, float from, float to)
        {
            rect.anchorMin = new Vector2(Mathf.Clamp01(from), 0f);
            rect.anchorMax = new Vector2(Mathf.Clamp01(to), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    public sealed class WorldOverlay
    {
        private const float FloatSeconds = 1f;

        private readonly Transform _root;
        private readonly List<BarView> _bars = new List<BarView>();
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
                    if (!unit.IsAlive || !services.View.Vision.IsVisible(unit) || unit.Kind == Data.Definitions.UnitKind.Ward) continue;
                    if (unit.IsStructure && unit.IsInvulnerable) continue;
                    Vector3 screen = camera.WorldToScreenPoint(unit.Position + Vector3.up * (unit.IsStructure ? 4f : 2.4f));
                    if (screen.z <= 0f) continue;
                    DrawBar(used++, unit, screen, services.View.LocalChampion);
                }
            for (int i = used; i < _bars.Count; i++) _bars[i].Hide();
            Animate(camera);
        }

        private void DrawBar(int index, Unit unit, Vector3 screen, Unit local)
        {
            if (index == _bars.Count) _bars.Add(new BarView(_root));
            bool ally = local != null ? unit.Team == local.Team : unit.Team == Team.Blue;
            Color color = unit.Team == Team.Neutral ? new Color(0.9f, 0.8f, 0.3f) : unit == local ? new Color(0.95f, 0.85f, 0.25f) : ally ? new Color(0.3f, 0.6f, 1f) : new Color(0.95f, 0.3f, 0.3f);
            _bars[index].Show(unit, screen, color);
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
