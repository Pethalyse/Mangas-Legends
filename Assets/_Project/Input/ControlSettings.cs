using System;
using System.Collections.Generic;
using MangaLegends.Simulation.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MangaLegends.Input
{
    public enum AimMode
    {
        Normal,
        QuickWithIndicator,
        Quick
    }

    // Player preferences (key bindings and cast mode per key), stored locally like the LoL client settings.
    public sealed class ControlSettings
    {
        public const string GameplayMap = "Gameplay";

        private const string BindingsKey = "ml.input.bindings";
        private const string ModeKeyPrefix = "ml.input.aim.";

        public static readonly InputButton[] AimableButtons =
        {
            InputButton.AbilityA, InputButton.AbilityZ, InputButton.AbilityE, InputButton.AbilityR,
            InputButton.Summoner1, InputButton.Summoner2, InputButton.AttackMove,
            InputButton.Item1, InputButton.Item2, InputButton.Item3, InputButton.Item4, InputButton.Item5, InputButton.Item6, InputButton.Trinket
        };

        public static readonly string[] RebindableActions =
        {
            "AbilityA", "AbilityZ", "AbilityE", "AbilityR", "Summoner1", "Summoner2",
            "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Trinket",
            "AttackMove", "Stop", "Recall", "LevelUpModifier", "SelfCastModifier",
            "Shop", "Scoreboard", "CameraLock", "CameraCenter"
        };

        private readonly Dictionary<InputButton, AimMode> _modes = new Dictionary<InputButton, AimMode>();
        private readonly InputActionAsset _controls;

        public ControlSettings(InputActionAsset controls)
        {
            _controls = controls;
            Load();
        }

        public InputActionAsset Controls => _controls;
        public InputActionMap Gameplay => _controls.FindActionMap(GameplayMap, true);

        public event Action Changed;

        public AimMode ModeOf(InputButton button) => _modes.TryGetValue(button, out AimMode mode) ? mode : AimMode.Normal;

        public void SetMode(InputButton button, AimMode mode)
        {
            _modes[button] = mode;
            PlayerPrefs.SetInt(ModeKeyPrefix + button, (int)mode);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public void SetAllModes(AimMode mode)
        {
            foreach (InputButton button in AimableButtons)
            {
                if (button == InputButton.AttackMove) continue;
                _modes[button] = mode;
                PlayerPrefs.SetInt(ModeKeyPrefix + button, (int)mode);
            }
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public void Load()
        {
            foreach (InputButton button in AimableButtons)
                _modes[button] = (AimMode)Mathf.Clamp(PlayerPrefs.GetInt(ModeKeyPrefix + button, (int)AimMode.Normal), 0, 2);
            string overrides = PlayerPrefs.GetString(BindingsKey, "");
            _controls.RemoveAllBindingOverrides();
            if (!string.IsNullOrEmpty(overrides)) _controls.LoadBindingOverridesFromJson(overrides);
        }

        public void SaveBindings()
        {
            PlayerPrefs.SetString(BindingsKey, _controls.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public void ResetAll()
        {
            _controls.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(BindingsKey);
            foreach (InputButton button in AimableButtons) PlayerPrefs.DeleteKey(ModeKeyPrefix + button);
            PlayerPrefs.Save();
            Load();
            Changed?.Invoke();
        }

        public string KeyLabel(string actionName)
        {
            InputAction action = Gameplay.FindAction(actionName, false);
            if (action == null || action.bindings.Count == 0) return "";
            return action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions).ToUpperInvariant();
        }

        public string KeyLabel(InputButton button) => KeyLabel(button.ToString());

        // Binding a key that another action already uses gives that action the old key, as the LoL client does.
        public InputActionRebindingExtensions.RebindingOperation Rebind(string actionName, Action<bool> completed)
        {
            InputAction action = Gameplay.FindAction(actionName, true);
            string previous = action.bindings[0].effectivePath;
            bool wasEnabled = _controls.enabled;
            _controls.Disable();
            InputActionRebindingExtensions.RebindingOperation operation = action.PerformInteractiveRebinding(0)
                .WithControlsExcluding("<Pointer>/position")
                .WithControlsExcluding("<Pointer>/delta")
                .WithControlsExcluding("<Mouse>/scroll")
                .WithControlsExcluding("<Mouse>/leftButton")
                .WithControlsExcluding("<Mouse>/rightButton")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.05f);
            operation.OnComplete(op =>
            {
                string chosen = action.bindings[0].effectivePath;
                foreach (InputAction other in Gameplay.actions)
                {
                    if (other == action || other.bindings.Count == 0 || Array.IndexOf(RebindableActions, other.name) < 0) continue;
                    if (other.bindings[0].effectivePath == chosen) other.ApplyBindingOverride(0, previous);
                }
                op.Dispose();
                if (wasEnabled) _controls.Enable();
                SaveBindings();
                completed?.Invoke(true);
            });
            operation.OnCancel(op =>
            {
                op.Dispose();
                if (wasEnabled) _controls.Enable();
                completed?.Invoke(false);
            });
            return operation.Start();
        }
    }
}
