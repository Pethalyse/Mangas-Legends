using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Presentation
{
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private MatchView _view;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 18f, -11f);
        [SerializeField] private float _panSpeed = 30f;
        [SerializeField] private float _edgePixels = 12f;
        [SerializeField] private float _minZoom = 0.6f;
        [SerializeField] private float _maxZoom = 1.4f;
        [SerializeField] private float _zoomStep = 0.0015f;
        [SerializeField] private Vector2 _bounds = new Vector2(80f, 80f);

        private Vector3 _focus;
        private float _zoom = 1f;
        private bool _locked = true;
        private bool _snapped;

        private void LateUpdate()
        {
            MatchServices services = _view.Services;
            if (services == null) return;
            LocalView local = services.View;
            if (local.TakeCameraLockToggle()) _locked = !_locked;
            _zoom = Mathf.Clamp(_zoom - local.TakeZoom() * _zoomStep, _minZoom, _maxZoom);

            Unit champion = _view.LocalChampion;
            if (champion != null && (_locked || local.CameraCenterHeld || !_snapped))
            {
                _focus = champion.transform.position;
                _snapped = true;
            }
            else EdgePan(local.PointerScreen);

            _focus.x = Mathf.Clamp(_focus.x, -_bounds.x, _bounds.x);
            _focus.z = Mathf.Clamp(_focus.z, -_bounds.y, _bounds.y);
            Transform cam = _view.Camera.transform;
            cam.position = _focus + _offset * _zoom;
            cam.rotation = Quaternion.LookRotation(_focus - cam.position);
        }

        private void EdgePan(Vector2 pointer)
        {
            Vector3 move = Vector3.zero;
            if (pointer.x <= _edgePixels) move.x -= 1f;
            if (pointer.x >= Screen.width - _edgePixels) move.x += 1f;
            if (pointer.y <= _edgePixels) move.z -= 1f;
            if (pointer.y >= Screen.height - _edgePixels) move.z += 1f;
            _focus += move * (_panSpeed * Time.deltaTime);
        }
    }
}
