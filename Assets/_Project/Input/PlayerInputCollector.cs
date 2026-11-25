using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Input;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using PlayerInput = MangaLegends.Simulation.Input.PlayerInput;

namespace MangaLegends.Input
{
    // The only place that reads devices: fills PlayerInput for Fusion and the local view state for camera and UI.
    // Cast keys follow the LoL flows: normal (key shows the indicator, left click casts), quick with indicator
    // (hold to aim, release casts) and quick (cast on press). A unit-targeted spell takes the unit under the cursor.
    public sealed class PlayerInputCollector : MonoBehaviour, INetworkRunnerCallbacks
    {
        private const float MoveRepeatSeconds = 0.15f;
        private const float RaycastDistance = 500f;

        private static readonly InputButton[] CastButtons =
        {
            InputButton.AbilityA, InputButton.AbilityZ, InputButton.AbilityE, InputButton.AbilityR,
            InputButton.Summoner1, InputButton.Summoner2, InputButton.Recall,
            InputButton.Item1, InputButton.Item2, InputButton.Item3, InputButton.Item4, InputButton.Item5, InputButton.Item6, InputButton.Trinket
        };

        [SerializeField] private InputActionAsset _controls;
        [SerializeField] private LayerMask _unitLayers = ~0;

        private readonly RaycastHit[] _hits = new RaycastHit[8];
        private readonly Dictionary<InputButton, InputAction> _buttons = new Dictionary<InputButton, InputAction>();
        private NetworkRunner _runner;
        private MatchServices _services;
        private InputAction _point;
        private InputAction _select;
        private InputAction _levelUp;
        private InputAction _selfCast;
        private InputAction _cancel;
        private InputAction _shop;
        private InputAction _scoreboard;
        private InputAction _cameraLock;
        private InputAction _cameraCenter;
        private InputAction _zoom;
        private NetworkButtons _pending;
        private Vector3 _movePoint;
        private NetworkId _moveTarget;
        private Vector3 _aimPoint;
        private NetworkId _aimTarget;
        private bool _hasAim;
        private sbyte _levelUpSlot = -1;
        private float _nextMoveRepeat;
        private bool _aiming;
        private InputButton _aimButton;
        private AimMode _aimMode;

        public ControlSettings Settings { get; private set; }

        private void Awake()
        {
            _runner = GetComponent<NetworkRunner>();
            _services = GetComponent<MatchServices>();
            Settings = new ControlSettings(_controls);
            InputActionMap map = Settings.Gameplay;
            _point = map.FindAction("Point", true);
            _select = map.FindAction("Select", true);
            _levelUp = map.FindAction("LevelUpModifier", true);
            _selfCast = map.FindAction("SelfCastModifier", true);
            _cancel = map.FindAction("Cancel", true);
            _shop = map.FindAction("Shop", true);
            _scoreboard = map.FindAction("Scoreboard", true);
            _cameraLock = map.FindAction("CameraLock", true);
            _cameraCenter = map.FindAction("CameraCenter", true);
            _zoom = map.FindAction("Zoom", true);
            foreach (InputButton button in Enum.GetValues(typeof(InputButton)))
            {
                InputAction action = map.FindAction(button.ToString(), false);
                if (action != null) _buttons[button] = action;
            }
        }

        private void OnEnable()
        {
            _controls.Enable();
            if (_runner != null) _runner.AddCallbacks(this);
        }

        private void OnDisable()
        {
            _controls.Disable();
            if (_runner != null) _runner.RemoveCallbacks(this);
        }

        private void Update()
        {
            LocalView view = _services.View;
            if (!_controls.enabled)
            {
                StopAiming(view);
                return;
            }
            if (_cancel.WasPressedThisFrame())
            {
                if (_aiming) StopAiming(view);
                else if (view.ShopOpen) view.ToggleShop();
                else view.ToggleMenu();
            }
            view.TooltipDetailHeld = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
            if (view.MenuOpen || IsTyping())
            {
                StopAiming(view);
                return;
            }

            view.PointerScreen = _point.ReadValue<Vector2>();
            view.PointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            view.ScoreboardHeld = _scoreboard.IsPressed();
            view.CameraCenterHeld = _cameraCenter.IsPressed();
            view.ZoomDelta += _zoom.ReadValue<Vector2>().y;
            if (_shop.WasPressedThisFrame()) view.ToggleShop();
            if (_cameraLock.WasPressedThisFrame()) view.ToggleCameraLock();
            UpdatePointer(view);

            CollectMove(view);
            if (_services.Commands.TakeMove(out Vector3 minimapPoint))
            {
                _pending.Set(InputButton.Move, true);
                _movePoint = minimapPoint;
                _moveTarget = default;
            }
            if (_buttons[InputButton.Stop].WasPressedThisFrame())
            {
                StopAiming(view);
                _pending.Set(InputButton.Stop, true);
            }
            CollectAttackMove(view);
            if (!_aiming && _select.WasPressedThisFrame() && !view.PointerOverUi) view.SelectedUnit = view.HoveredUnit;
            foreach (InputButton button in CastButtons) CollectCast(button, view);
            UpdateAiming(view);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            LocalView view = _services.View;
            PlayerInput data = PlayerInput.Empty;
            data.Buttons = _pending;
            data.MovePoint = _movePoint;
            data.MoveTarget = _moveTarget;
            data.AimPoint = _hasAim ? _aimPoint : view.PointerWorld;
            data.AimTarget = _hasAim ? _aimTarget : view.HoveredUnit != null ? view.HoveredUnit.Object.Id : default;
            data.BuyItem = (short)_services.Commands.TakeBuy();
            data.SellSlot = (sbyte)_services.Commands.TakeSell();
            data.TrinketChoice = (sbyte)_services.Commands.TakeTrinket();
            int uiLevelUp = _services.Commands.TakeLevelUp();
            data.LevelUpSlot = uiLevelUp >= 0 ? (sbyte)uiLevelUp : _levelUpSlot;
            input.Set(data);
            _pending = default;
            _hasAim = false;
            _levelUpSlot = -1;
        }

        // Typing in a text field (shop search, player name) must not cast spells.
        private static bool IsTyping()
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            UnityEngine.UI.InputField field = selected != null ? selected.GetComponent<UnityEngine.UI.InputField>() : null;
            return field != null && field.isFocused;
        }

        private void CollectMove(LocalView view)
        {
            InputAction move = _buttons[InputButton.Move];
            if (move.WasPressedThisFrame() && _aiming)
            {
                StopAiming(view);
                return;
            }
            if (view.PointerOverUi) return;
            bool repeat = move.IsPressed() && Time.unscaledTime >= _nextMoveRepeat;
            if (!move.WasPressedThisFrame() && !repeat) return;
            _pending.Set(InputButton.Move, true);
            _movePoint = view.PointerWorld;
            _moveTarget = view.HoveredUnit != null ? view.HoveredUnit.Object.Id : default;
            _nextMoveRepeat = Time.unscaledTime + MoveRepeatSeconds;
        }

        private void CollectAttackMove(LocalView view)
        {
            if (!_buttons[InputButton.AttackMove].WasPressedThisFrame() || view.LocalChampion == null) return;
            AimMode mode = Settings.ModeOf(InputButton.AttackMove);
            if (mode == AimMode.Quick) FireAttackMove(view);
            else StartAiming(InputButton.AttackMove, mode, view);
        }

        private void CollectCast(InputButton button, LocalView view)
        {
            if (!_buttons.TryGetValue(button, out InputAction action) || !action.WasPressedThisFrame()) return;
            if (_levelUp.IsPressed() && AimSource.TryCasterSlot(button, out CasterSlot levelSlot) && levelSlot <= CasterSlot.R)
            {
                _levelUpSlot = (sbyte)levelSlot;
                return;
            }

            AimSource source = AimSource.For(view.LocalChampion, button);
            if (!source.IsValid) return;
            if (!source.NeedsAim)
            {
                Fire(button, view, false);
                return;
            }
            if (_selfCast.IsPressed() && CanSelfCast(source.Targeting))
            {
                Fire(button, view, true);
                return;
            }
            AimMode mode = Settings.ModeOf(button);
            if (mode == AimMode.Quick || !source.Ready) Fire(button, view, false);
            else StartAiming(button, mode, view);
        }

        private void UpdateAiming(LocalView view)
        {
            view.AimTarget = _aiming && _aimButton != InputButton.AttackMove ? view.HoveredUnit : null;
            if (!_aiming) return;
            if (view.LocalChampion == null || !view.LocalChampion.IsAlive)
            {
                StopAiming(view);
                return;
            }
            bool isAttackMove = _aimButton == InputButton.AttackMove;
            if (!isAttackMove && !AimSource.For(view.LocalChampion, _aimButton).IsValid)
            {
                StopAiming(view);
                return;
            }

            bool release = _aimMode == AimMode.QuickWithIndicator && _buttons[_aimButton].WasReleasedThisFrame();
            bool click = _select.WasPressedThisFrame() && !view.PointerOverUi;
            if (!release && !click) return;
            if (isAttackMove) FireAttackMove(view);
            else if (click && !release && NeedsUnderCursorTarget(view)) return;
            else Fire(_aimButton, view, false);
            StopAiming(view);
        }

        private bool NeedsUnderCursorTarget(LocalView view)
        {
            AimSource source = AimSource.For(view.LocalChampion, _aimButton);
            if (source.Targeting.Mode != TargetingMode.Unit) return false;
            Unit hovered = view.HoveredUnit;
            return hovered == null || !TeamRules.Matches(source.Targeting.ValidTargets, view.LocalChampion, hovered);
        }

        private void StartAiming(InputButton button, AimMode mode, LocalView view)
        {
            _aiming = true;
            _aimButton = button;
            _aimMode = mode;
            view.IsAiming = true;
            view.AimButton = button;
        }

        private void StopAiming(LocalView view)
        {
            _aiming = false;
            view.IsAiming = false;
        }

        private void Fire(InputButton button, LocalView view, bool onSelf)
        {
            _pending.Set(button, true);
            _hasAim = true;
            _aimPoint = view.PointerWorld;
            Unit target = onSelf ? view.LocalChampion : view.HoveredUnit;
            _aimTarget = target != null && target.Object != null ? target.Object.Id : default;
        }

        private void FireAttackMove(LocalView view)
        {
            _pending.Set(InputButton.AttackMove, true);
            _movePoint = view.PointerWorld;
            _moveTarget = default;
        }

        private static bool CanSelfCast(TargetingData targeting) =>
            targeting.Mode == TargetingMode.Unit && (targeting.ValidTargets & (TeamFilter.Self | TeamFilter.Allies)) != 0;

        private void UpdatePointer(LocalView view)
        {
            Camera camera = view.Camera;
            view.HoveredUnit = null;
            if (camera == null) return;
            Ray ray = camera.ScreenPointToRay(view.PointerScreen);
            if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance)) view.PointerWorld = ray.GetPoint(distance);
            int count = Physics.RaycastNonAlloc(ray, _hits, RaycastDistance, _unitLayers, QueryTriggerInteraction.Collide);
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Unit unit = _hits[i].collider.GetComponentInParent<Unit>();
                if (unit == null || unit.Object == null || !unit.IsAlive || !view.Vision.IsVisible(unit) || _hits[i].distance >= best) continue;
                best = _hits[i].distance;
                view.HoveredUnit = unit;
            }
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}
