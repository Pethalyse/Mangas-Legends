using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // Training game (solo) test tools: levels, unlimited gold, no cooldowns, enemy and allied dummies.
    public sealed class TrainingPanel
    {
        private readonly GameObject _root;
        private readonly Text _gold;
        private readonly Text _cooldowns;
        private MatchServices _services;

        public TrainingPanel(Transform canvas)
        {
            Image panel = UiFactory.Box(canvas, "Training", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 120f), new Vector2(190f, 330f), new Color(0.03f, 0.05f, 0.08f, 0.9f));
            _root = panel.gameObject;
            Text title = UiFactory.Text(panel.transform, "Title", "ENTRAÎNEMENT", 16, TextAnchor.UpperCenter, UiFactory.Accent);
            title.rectTransform.offsetMax = new Vector2(0f, -6f);
            float y = -34f;
            Add(panel.transform, "Niveau +1", () => Tools()?.AddLevels(Champion(), 1), ref y);
            Add(panel.transform, "Niveau 18", () => Tools()?.AddLevels(Champion(), 18), ref y);
            _gold = Add(panel.transform, "Or illimité", () => { var t = Tools(); if (t != null) t.InfiniteGold = !t.InfiniteGold; }, ref y);
            _cooldowns = Add(panel.transform, "Sans recharge", () => { var t = Tools(); if (t != null) t.NoCooldowns = !t.NoCooldowns; }, ref y);
            Add(panel.transform, "Pantin ennemi", () => SpawnDummy(true), ref y);
            Add(panel.transform, "Pantin allié", () => SpawnDummy(false), ref y);
            Add(panel.transform, "Retirer les pantins", () => Tools()?.ClearDummies(), ref y);
            _root.SetActive(false);
        }

        public void Refresh(MatchServices services)
        {
            _services = services;
            TrainingTools tools = Tools();
            bool visible = tools != null && tools.Available && services.Match.Phase == MatchPhase.Playing && Champion() != null;
            _root.SetActive(visible);
            if (!visible) return;
            _gold.text = tools.InfiniteGold ? "Or illimité : oui" : "Or illimité : non";
            _cooldowns.text = tools.NoCooldowns ? "Sans recharge : oui" : "Sans recharge : non";
        }

        private Text Add(Transform parent, string label, System.Action onClick, ref float y)
        {
            Button button = UiFactory.Button(parent, label, label, onClick, 15);
            UiFactory.Place(button, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(170f, 34f));
            y -= 40f;
            return button.GetComponentInChildren<Text>();
        }

        private TrainingTools Tools() => _services != null && _services.Match != null ? _services.Match.GetComponent<TrainingTools>() : null;

        private Unit Champion() => _services != null ? _services.View.LocalChampion : null;

        // Dummies appear a few metres in front of the champion.
        private void SpawnDummy(bool enemy)
        {
            Unit champion = Champion();
            if (champion == null) return;
            Team team = enemy ? TeamRules.Opponent(champion.Team) : champion.Team;
            Tools()?.SpawnDummy(team, champion.Position + champion.transform.forward * 4f);
        }
    }
}
