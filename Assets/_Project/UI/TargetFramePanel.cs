using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // Top-left frame of the player's current target (selected with a left click, else the unit being attacked):
    // any unit, minions and wards included, with its health, mana and effects (Kirito's marks, Giorno's flowers).
    public sealed class TargetFramePanel
    {
        private const int EffectSlots = 8;

        private readonly GameObject _root;
        private readonly Text _name;
        private readonly Text _level;
        private readonly Image _health;
        private readonly Text _healthText;
        private readonly Image _mana;
        private readonly Text[] _effects = new Text[EffectSlots];

        public TargetFramePanel(Transform canvas)
        {
            Image frame = UiFactory.Box(canvas, "TargetFrame", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -12f), new Vector2(300f, 104f), new Color(0.03f, 0.04f, 0.06f, 0.9f));
            _root = frame.gameObject;
            _name = UiFactory.Text(frame.transform, "Name", "", 18, TextAnchor.UpperLeft, Color.white);
            UiFactory.Place(_name, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -6f), new Vector2(230f, 24f));
            _level = UiFactory.Text(frame.transform, "Level", "", 16, TextAnchor.UpperRight, new Color(0.78f, 0.67f, 0.43f));
            UiFactory.Place(_level, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -8f), new Vector2(60f, 22f));
            UiFactory.Bar(frame.transform, "Health", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -34f), new Vector2(280f, 18f), UiFactory.Health, out _health);
            _healthText = UiFactory.Text(_health.transform.parent, "Value", "", 13, TextAnchor.MiddleCenter, Color.white);
            UiFactory.Bar(frame.transform, "Mana", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -54f), new Vector2(280f, 8f), UiFactory.Mana, out _mana);
            for (int i = 0; i < EffectSlots; i++)
            {
                Image box = UiFactory.Box(frame.transform, "Effect" + i, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f + i * 35f, -68f), new Vector2(32f, 30f), new Color(0.25f, 0.3f, 0.45f));
                _effects[i] = UiFactory.Text(box.transform, "Label", "", 12, TextAnchor.MiddleCenter, Color.white);
            }
            _root.SetActive(false);
        }

        public void Refresh(LocalView view)
        {
            Unit target = view.SelectedUnit;
            Unit champion = view.LocalChampion;
            if ((target == null || target.Object == null || !target.IsAlive) && champion != null && champion.Object != null && champion.Attack != null)
                champion.TryResolve(champion.Attack.Target, out target);
            bool show = target != null && target.Object != null && target.Definition != null && target.IsAlive && view.Vision.IsVisible(target);
            _root.SetActive(show);
            if (!show) return;

            _name.text = target.Definition.DisplayName;
            _level.text = target.IsChampion ? $"niv. {target.Stats.Level}" : "";
            UiFactory.SetFill(_health, target.Health.Ratio);
            _healthText.text = $"{Mathf.CeilToInt(target.Health.Current)} / {Mathf.CeilToInt(target.Health.Max)}";
            bool mana = target.Resource != null && target.Resource.UsesResource;
            _mana.transform.parent.gameObject.SetActive(mana);
            if (mana) UiFactory.SetFill(_mana, target.Resource.Ratio);

            StatusEffects status = target.Status;
            int used = 0;
            for (int slot = 0; slot < status.Active.Length && used < EffectSlots; slot++)
            {
                StatusEffectDefinition definition = status.DefinitionAt(slot);
                if (definition == null) continue;
                Text label = _effects[used++];
                label.transform.parent.gameObject.SetActive(true);
                string name = definition.DisplayName ?? "";
                int stacks = status.Active[slot].Stacks;
                label.text = (name.Length > 3 ? name.Substring(0, 3) : name) + (stacks > 1 || definition.MaxStacks > 1 ? "\n" + stacks : "");
                ((Image)label.transform.parent.GetComponent<Image>()).color = definition.IsCrowdControl ? new Color(0.6f, 0.2f, 0.2f) : new Color(0.25f, 0.3f, 0.45f);
            }
            for (int i = used; i < EffectSlots; i++) _effects[i].transform.parent.gameObject.SetActive(false);
        }
    }
}
