using Fusion;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Presentation
{
    // Scene entry point of the presentation layer: exposes the runner's services once the match object is spawned.
    public sealed class MatchView : MonoBehaviour
    {
        [SerializeField] private MatchController _match;
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _minimapExcluded = 1 << 8;
        [SerializeField] private int _minimapResolution = 512;
        [SerializeField] private Shader _grayscaleShader;
        [SerializeField] private LayerMask _sightBlockers = 1 << 9;

        private RenderTexture _minimap;

        public MatchController Match => _match;
        public NetworkRunner Runner => _match != null && _match.Object != null ? _match.Runner : null;
        public MatchServices Services => Runner != null ? MatchServices.Of(Runner) : null;
        public Camera Camera => _camera;
        public Unit LocalChampion { get; private set; }
        public bool IsReady => Services != null;
        public Vector2 MapSize => _match != null && _match.Map != null ? _match.Map.Size : new Vector2(160f, 160f);

        public Texture Minimap => _minimap != null ? _minimap : _minimap = MinimapCapture.Render(MapSize, _minimapResolution, _minimapExcluded);

        private void OnDestroy()
        {
            if (_minimap != null) _minimap.Release();
        }

        private void Update()
        {
            MatchServices services = Services;
            if (services == null) return;
            services.View.Camera = _camera;
            if (LocalChampion == null || LocalChampion.Object == null) LocalChampion = FindLocalChampion(services);
            services.View.LocalChampion = LocalChampion;
            UpdateDeathView();
            services.View.Vision.Update(LocalChampion != null ? LocalChampion.Team : Team.None, services.Units.All, _match != null ? _match.Map : null, _sightBlockers);
        }

        private DeathGrayscale _grayscale;

        private void UpdateDeathView()
        {
            bool dead = LocalChampion != null && LocalChampion.Object != null && !LocalChampion.IsAlive;
            if (_grayscale == null && !_camera.TryGetComponent(out _grayscale))
            {
                _grayscale = _camera.gameObject.AddComponent<DeathGrayscale>();
                _grayscale.Configure(_grayscaleShader);
            }
            _grayscale.enabled = dead;
        }

        private static Unit FindLocalChampion(MatchServices services)
        {
            foreach (Unit unit in services.Units.All)
                if (unit != null && unit.Object != null && unit.IsChampion && unit.Object.HasInputAuthority) return unit;
            return null;
        }
    }
}
