using Fusion;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Match;
using UnityEngine;

namespace MangaLegends.Simulation.Services
{
    // Runner-scoped service hub: one per NetworkRunner, reached through runner.GetComponent, never static.
    public sealed class MatchServices : MonoBehaviour
    {
        [SerializeField] private ContentRegistry _content;

        private readonly UnitRegistry _units = new UnitRegistry();
        private readonly LocalCommands _commands = new LocalCommands();
        private readonly LocalView _view = new LocalView();

        public ContentRegistry Content => _content;
        public GameRulesConfig Rules => _content != null ? _content.Rules : null;
        public UnitRegistry Units => _units;
        public LocalCommands Commands => _commands;
        public LocalView View => _view;
        public MapLayout Map { get; private set; }
        public MatchController Match { get; private set; }

        public static MatchServices Of(NetworkRunner runner) => runner != null ? runner.GetComponent<MatchServices>() : null;

        public void SetContent(ContentRegistry content) => _content = content;

        public void Attach(MatchController match, MapLayout map)
        {
            Match = match;
            Map = map;
        }

        public void Detach(MatchController match)
        {
            if (Match != match) return;
            Match = null;
            Map = null;
        }
    }
}
