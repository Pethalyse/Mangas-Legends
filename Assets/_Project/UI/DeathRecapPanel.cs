using System.Collections.Generic;
using MangaLegends.Core.Combat;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // LoL death recap: who dealt how much to the local champion during the seconds before its death.
    public sealed class DeathRecapPanel
    {
        private const float WindowSeconds = 15f;
        private const int Lines = 6;

        private readonly GameObject _root;
        private readonly Text _title;
        private readonly Text[] _lines = new Text[Lines];
        private readonly List<(float time, NetworkIdName source, float amount, DamageType type)> _log = new List<(float, NetworkIdName, float, DamageType)>();
        private readonly Dictionary<string, (float physical, float magical, float pure)> _totals = new Dictionary<string, (float, float, float)>();
        private Unit _champion;
        private int _seen = -1;
        private bool _wasAlive = true;

        public DeathRecapPanel(Transform canvas)
        {
            Image panel = UiFactory.Box(canvas, "DeathRecap", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(240f, -40f), new Vector2(420f, 250f), new Color(0.04f, 0.04f, 0.06f, 0.92f));
            _root = panel.gameObject;
            _title = UiFactory.Text(panel.transform, "Title", "", 22, TextAnchor.UpperCenter, new Color(1f, 0.55f, 0.5f));
            UiFactory.Place(_title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(400f, 30f));
            for (int i = 0; i < Lines; i++)
            {
                _lines[i] = UiFactory.Text(panel.transform, "Line" + i, "", 17, TextAnchor.MiddleLeft, Color.white);
                UiFactory.Place(_lines[i], new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -46f - i * 32f), new Vector2(392f, 30f));
            }
            _root.SetActive(false);
        }

        public void Refresh(Unit champion)
        {
            if (champion != _champion)
            {
                _champion = champion;
                _seen = -1;
                _log.Clear();
            }
            if (champion == null || champion.Object == null)
            {
                _root.SetActive(false);
                return;
            }
            Collect(champion);
            bool alive = champion.IsAlive;
            if (_wasAlive && !alive) Build(champion);
            _wasAlive = alive;
            _root.SetActive(!alive);
        }

        private void Collect(Unit champion)
        {
            Health health = champion.Health;
            int count = health.HitCount;
            if (_seen < 0) _seen = count;
            if (count - _seen > Health.HitHistory) _seen = count - Health.HitHistory;
            for (int sequence = _seen; sequence < count; sequence++)
            {
                HitRecord hit = health.HitAt(sequence);
                if (hit.IsHeal || hit.Amount <= 0f) continue;
                _log.Add((Time.unscaledTime, new NetworkIdName(champion, hit.Source), hit.Amount, (DamageType)hit.Type));
            }
            _seen = count;
            _log.RemoveAll(entry => Time.unscaledTime - entry.time > WindowSeconds);
        }

        private void Build(Unit champion)
        {
            _totals.Clear();
            float sum = 0f;
            foreach (var entry in _log)
            {
                _totals.TryGetValue(entry.source.Name, out var t);
                if (entry.type == DamageType.Magical) t.magical += entry.amount;
                else if (entry.type == DamageType.True) t.pure += entry.amount;
                else t.physical += entry.amount;
                _totals[entry.source.Name] = t;
                sum += entry.amount;
            }
            _title.text = $"Récapitulatif de la mort — {Mathf.RoundToInt(sum)} dégâts";
            var ordered = new List<KeyValuePair<string, (float physical, float magical, float pure)>>(_totals);
            ordered.Sort((a, b) => (b.Value.physical + b.Value.magical + b.Value.pure).CompareTo(a.Value.physical + a.Value.magical + a.Value.pure));
            for (int i = 0; i < Lines; i++)
            {
                if (i >= ordered.Count) { _lines[i].text = ""; continue; }
                var t = ordered[i].Value;
                _lines[i].text = $"{ordered[i].Key} : {Mathf.RoundToInt(t.physical + t.magical + t.pure)}" +
                                 $"  <color=#ff9a4d>{Mathf.RoundToInt(t.physical)}</color> / <color=#a970ff>{Mathf.RoundToInt(t.magical)}</color> / {Mathf.RoundToInt(t.pure)}";
            }
        }

        private readonly struct NetworkIdName
        {
            public NetworkIdName(Unit champion, Fusion.NetworkId source)
            {
                Name = champion.TryResolve(source, out Unit unit) && unit.Definition != null ? unit.Definition.DisplayName : "Fontaine";
            }

            public string Name { get; }
        }
    }
}
