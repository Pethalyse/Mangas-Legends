using MangaLegends.Simulation.Input;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Services
{
    // Local, non-simulated view state shared between the input collector, the camera and the UI.
    public sealed class LocalView
    {
        private int _shopToggles;
        private int _cameraLockToggles;
        private int _menuToggles;

        public Camera Camera { get; set; }
        public Unit LocalChampion { get; set; }
        public Vector2 PointerScreen { get; set; }
        public Vector3 PointerWorld { get; set; }
        public Unit HoveredUnit { get; set; }
        public bool ScoreboardHeld { get; set; }
        public bool CameraCenterHeld { get; set; }
        public float ZoomDelta { get; set; }
        public bool PointerOverUi { get; set; }
        public bool IsAiming { get; set; }
        public InputButton AimButton { get; set; }
        public Unit AimTarget { get; set; }
        public bool MenuOpen { get; set; }
        public TeamVision Vision { get; } = new TeamVision();
        public Vector3? CameraFocusRequest { get; set; }

        public void ToggleShop() => _shopToggles++;
        public void ToggleCameraLock() => _cameraLockToggles++;
        public void ToggleMenu() => _menuToggles++;

        public bool TakeShopToggle() => Take(ref _shopToggles);
        public bool TakeCameraLockToggle() => Take(ref _cameraLockToggles);
        public bool TakeMenuToggle() => Take(ref _menuToggles);

        public float TakeZoom()
        {
            float zoom = ZoomDelta;
            ZoomDelta = 0f;
            return zoom;
        }

        private static bool Take(ref int counter)
        {
            bool toggled = counter % 2 == 1;
            counter = 0;
            return toggled;
        }
    }
}
