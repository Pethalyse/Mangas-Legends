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

        public MatchController Match => _match;
        public NetworkRunner Runner => _match != null && _match.Object != null ? _match.Runner : null;
        public MatchServices Services => Runner != null ? MatchServices.Of(Runner) : null;
        public Camera Camera => _camera;
        public Unit LocalChampion { get; private set; }
        public bool IsReady => Services != null;

        private void Update()
        {
            MatchServices services = Services;
            if (services == null) return;
            services.View.Camera = _camera;
            if (LocalChampion == null || LocalChampion.Object == null) LocalChampion = FindLocalChampion(services);
            services.View.LocalChampion = LocalChampion;
            services.View.Vision.Update(LocalChampion != null ? LocalChampion.Team : Team.None, services.Units.All);
        }

        private static Unit FindLocalChampion(MatchServices services)
        {
            foreach (Unit unit in services.Units.All)
                if (unit != null && unit.Object != null && unit.IsChampion && unit.Object.HasInputAuthority) return unit;
            return null;
        }
    }
}
