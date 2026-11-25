using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
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
    public sealed class PlayerInputCollector : MonoBehaviour, INetworkRunnerCallbacks
    {
        private const float MoveRepeatSeconds = 0.15f;
        private const float RaycastDistance = 500f;

        [SerializeField] private InputActionAsset _controls;
        [SerializeField] private LayerMask _unitLayers = ~0;

        private readonly RaycastHit[] _hits = new RaycastHit[8];
        private readonly Dictionary<InputButton, InputAction> _buttons = new Dictionary<InputButton, InputAction>();
        private NetworkRunner _runner;
        private MatchServices _services;
        private InputAction _point;
        private InputAction _levelUp;
        private InputAction _shop;
        private InputAction _scoreboard;
        private InputAction _cameraLock;
        private InputAction _cameraCenter;
        private InputAction _zoom;
        private NetworkButtons _pending;
        private Vector3 _movePoint;
        private NetworkId _moveTarget;
        private sbyte _levelUpSlot = -1;
        private float _nextMoveRepeat;

        private void Awake()
        {
            _runner = GetComponent<NetworkRunner>();
            _services = GetComponent<MatchServices>();
            InputActionMap map = _controls.FindActionMap("Gameplay", true);
            _point = map.FindAction("Point", true);
            _levelUp = map.FindAction("LevelUpModifier", true);
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
            _buttons[InputButton.Move] = map.FindAction("Move", true);
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
            view.PointerScreen = _point.ReadValue<Vector2>();
            view.PointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            view.ScoreboardHeld = _scoreboard.IsPressed();
            view.CameraCenterHeld = _cameraCenter.IsPressed();
            view.ZoomDelta += _zoom.ReadValue<Vector2>().y;
            if (_shop.WasPressedThisFrame()) view.ToggleShop();
            if (_cameraLock.WasPressedThisFrame()) view.ToggleCameraLock();
            UpdatePointer(view);

            if (!view.PointerOverUi)
            {
                InputAction move = _buttons[InputButton.Move];
                bool repeat = move.IsPressed() && Time.unscaledTime >= _nextMoveRepeat;
                if (move.WasPressedThisFrame() || repeat)
                {
                    _pending.Set(InputButton.Move, true);
                    _movePoint = view.PointerWorld;
                    _moveTarget = view.HoveredUnit != null ? view.HoveredUnit.Object.Id : default;
                    _nextMoveRepeat = Time.unscaledTime + MoveRepeatSeconds;
                }
            }

            CollectPress(InputButton.AttackMove, view);
            CollectPress(InputButton.Stop, view);
            CollectAbility(InputButton.AbilityA, 1);
            CollectAbility(InputButton.AbilityZ, 2);
            CollectAbility(InputButton.AbilityE, 3);
            CollectAbility(InputButton.AbilityR, 4);
            CollectPress(InputButton.Summoner1, view);
            CollectPress(InputButton.Summoner2, view);
            CollectPress(InputButton.Recall, view);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            LocalView view = _services.View;
            PlayerInput data = PlayerInput.Empty;
            data.Buttons = _pending;
            data.MovePoint = _movePoint;
            data.MoveTarget = _moveTarget;
            data.AimPoint = view.PointerWorld;
            data.AimTarget = view.HoveredUnit != null ? view.HoveredUnit.Object.Id : default;
            data.BuyItem = (short)_services.Commands.TakeBuy();
            data.SellSlot = (sbyte)_services.Commands.TakeSell();
            int uiLevelUp = _services.Commands.TakeLevelUp();
            data.LevelUpSlot = uiLevelUp >= 0 ? (sbyte)uiLevelUp : _levelUpSlot;
            input.Set(data);
            _pending = default;
            _levelUpSlot = -1;
        }

        private void CollectPress(InputButton button, LocalView view)
        {
            if (!_buttons.TryGetValue(button, out InputAction action) || !action.WasPressedThisFrame()) return;
            _pending.Set(button, true);
            if (button == InputButton.AttackMove) _movePoint = view.PointerWorld;
        }

        private void CollectAbility(InputButton button, int slot)
        {
            if (!_buttons.TryGetValue(button, out InputAction action) || !action.WasPressedThisFrame()) return;
            if (_levelUp.IsPressed()) _levelUpSlot = (sbyte)slot;
            else _pending.Set(button, true);
        }

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
                if (unit == null || unit.Object == null || !unit.IsAlive || _hits[i].distance >= best) continue;
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
