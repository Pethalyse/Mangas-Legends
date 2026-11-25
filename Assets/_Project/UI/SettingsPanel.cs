using System;
using System.Collections.Generic;
using MangaLegends.Input;
using MangaLegends.Simulation.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // Options page modelled on the LoL hotkey settings: one key per action and a cast mode per cast key.
    public sealed class SettingsPanel
    {
        private const float RowHeight = 32f;

        private static readonly Dictionary<string, string> ActionLabels = new Dictionary<string, string>
        {
            { "AbilityA", "Compétence 1" }, { "AbilityZ", "Compétence 2" }, { "AbilityE", "Compétence 3" }, { "AbilityR", "Ultime" },
            { "Summoner1", "Sort d'invocateur 1" }, { "Summoner2", "Sort d'invocateur 2" },
            { "Item1", "Objet 1" }, { "Item2", "Objet 2" }, { "Item3", "Objet 3" }, { "Item4", "Objet 4" }, { "Item5", "Objet 5" }, { "Item6", "Objet 6" },
            { "AttackMove", "Attaque-déplacement" }, { "Stop", "Arrêter" }, { "Recall", "Rappel" },
            { "LevelUpModifier", "Monter un rang (+ touche)" }, { "SelfCastModifier", "Lancer sur soi (+ touche)" },
            { "Shop", "Boutique" }, { "Scoreboard", "Scores (maintenir)" }, { "CameraLock", "Verrouiller la caméra" }, { "CameraCenter", "Centrer la caméra (maintenir)" }
        };

        private static readonly string[] ModeLabels = { "Normal", "Rapide + indicateur", "Rapide" };
        private static readonly string[] AttackMoveLabels = { "Clic pour valider", "Au relâchement", "Sur le curseur" };

        private readonly ControlSettings _settings;
        private readonly GameObject _root;
        private readonly Dictionary<string, Text> _keyTexts = new Dictionary<string, Text>();
        private readonly Dictionary<InputButton, Text> _modeTexts = new Dictionary<InputButton, Text>();
        private readonly Text _hint;
        private InputActionRebindingExtensions.RebindingOperation _rebinding;

        public SettingsPanel(Transform owner, ControlSettings settings, Action leave)
        {
            _settings = settings;
            Canvas canvas = UiFactory.Canvas(owner, "SettingsUi", 50);
            Image shade = UiFactory.Image(canvas.transform, "Settings", new Color(0f, 0f, 0f, 0.6f));
            _root = canvas.gameObject;
            Image panel = UiFactory.Box(shade.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240f, 900f), new Color(0.06f, 0.07f, 0.1f, 1f));
            Transform root = panel.transform;

            Text title = UiFactory.Text(root, "Title", "OPTIONS — RACCOURCIS", 36, TextAnchor.MiddleCenter, UiFactory.Accent);
            UiFactory.Place(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(1000f, 50f));

            Header(root, "Touches (clique puis appuie sur une touche, Échap annule)", 30f);
            for (int i = 0; i < ControlSettings.RebindableActions.Length; i++)
            {
                string action = ControlSettings.RebindableActions[i];
                float y = -110f - i * RowHeight;
                Label(root, ActionLabels.TryGetValue(action, out string label) ? label : action, 30f, y, 320f);
                Button button = UiFactory.Button(root, action, "", () => StartRebind(action), 20);
                UiFactory.Place(button, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, y), new Vector2(200f, RowHeight - 4f));
                _keyTexts[action] = button.GetComponentInChildren<Text>();
            }

            Header(root, "Mode de lancement", 640f);
            for (int i = 0; i < ControlSettings.AimableButtons.Length; i++)
            {
                InputButton cast = ControlSettings.AimableButtons[i];
                float y = -110f - i * RowHeight;
                Label(root, ActionLabels.TryGetValue(cast.ToString(), out string label) ? label : cast.ToString(), 640f, y, 280f);
                Button button = UiFactory.Button(root, "Mode" + cast, "", () => CycleMode(cast), 20);
                UiFactory.Place(button, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(930f, y), new Vector2(280f, RowHeight - 4f));
                _modeTexts[cast] = button.GetComponentInChildren<Text>();
            }

            float buttonsY = -110f - ControlSettings.AimableButtons.Length * RowHeight - 20f;
            AddButton(root, "Tout en normal", () => _settings.SetAllModes(AimMode.Normal), 640f, buttonsY, 280f);
            AddButton(root, "Tout en rapide + indicateur", () => _settings.SetAllModes(AimMode.QuickWithIndicator), 930f, buttonsY, 280f);
            AddButton(root, "Tout en rapide", () => _settings.SetAllModes(AimMode.Quick), 640f, buttonsY - 44f, 280f);
            AddButton(root, "Réinitialiser tout", () => _settings.ResetAll(), 930f, buttonsY - 44f, 280f);

            _hint = UiFactory.Text(root, "Hint", "", 20, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.7f));
            UiFactory.Place(_hint, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 20f), new Vector2(700f, 60f));
            _hint.text = "Normal : la touche affiche l'indicateur, clic gauche pour lancer, clic droit ou Échap pour annuler.\n" +
                         "Rapide + indicateur : maintenir pour viser, relâcher pour lancer. Rapide : lance au curseur.";

            AddButton(root, "Fermer", Hide, 1220f - 230f, -900f + 70f, 200f);
            if (leave != null) AddButton(root, "Quitter la partie", () => leave(), 1220f - 450f, -900f + 70f, 200f);

            _settings.Changed += Refresh;
            Refresh();
            _root.SetActive(false);
        }

        public bool IsOpen => _root.activeSelf;

        public void Show()
        {
            _root.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            _rebinding?.Cancel();
            _root.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen) Hide();
            else Show();
        }

        public void Dispose() => _settings.Changed -= Refresh;

        private void StartRebind(string action)
        {
            _rebinding?.Cancel();
            _keyTexts[action].text = "…";
            _rebinding = _settings.Rebind(action, _ =>
            {
                _rebinding = null;
                Refresh();
            });
        }

        private void CycleMode(InputButton button)
        {
            AimMode next = (AimMode)(((int)_settings.ModeOf(button) + 1) % 3);
            _settings.SetMode(button, next);
        }

        private void Refresh()
        {
            foreach (KeyValuePair<string, Text> entry in _keyTexts)
            {
                string label = _settings.KeyLabel(entry.Key);
                entry.Value.text = string.IsNullOrEmpty(label) ? "—" : label;
            }
            foreach (KeyValuePair<InputButton, Text> entry in _modeTexts)
            {
                int mode = (int)_settings.ModeOf(entry.Key);
                entry.Value.text = entry.Key == InputButton.AttackMove ? AttackMoveLabels[mode] : ModeLabels[mode];
            }
        }

        private static void Header(Transform root, string text, float x)
        {
            Text header = UiFactory.Text(root, text, text, 22, TextAnchor.MiddleLeft, UiFactory.Accent);
            UiFactory.Place(header, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -66f), new Vector2(580f, 34f));
        }

        private static void Label(Transform root, string text, float x, float y, float width)
        {
            Text label = UiFactory.Text(root, text, text, 20, TextAnchor.MiddleLeft, Color.white);
            UiFactory.Place(label, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(width, RowHeight - 4f));
        }

        private static void AddButton(Transform root, string text, Action onClick, float x, float y, float width)
        {
            Button button = UiFactory.Button(root, text, text, onClick, 20);
            UiFactory.Place(button, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(width, 38f));
        }
    }
}
