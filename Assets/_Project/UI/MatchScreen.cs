using MangaLegends.Input;
using MangaLegends.Networking;
using MangaLegends.Presentation;
using MangaLegends.Simulation.Services;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    public sealed class MatchScreen : MonoBehaviour
    {
        [SerializeField] private MatchView _view;

        private LobbyPanel _lobby;
        private HudPanel _hud;
        private ShopPanel _shop;
        private ScoreboardPanel _scoreboard;
        private TeamBarPanel _teamBar;
        private MinimapPanel _minimap;
        private AnnouncementPanel _announcements;
        private DeathRecapPanel _deathRecap;
        private WorldOverlay _overlay;
        private TargetFramePanel _targetFrame;
        private Text _waiting;
        private SettingsPanel _settings;
        private ControlSettings _controlSettings;

        private void Start()
        {
            Canvas canvas = UiFactory.Canvas(transform, "MatchUi", 10);
            _overlay = new WorldOverlay(canvas.transform);
            _hud = new HudPanel(canvas.transform);
            _teamBar = new TeamBarPanel(canvas.transform);
            _targetFrame = new TargetFramePanel(canvas.transform);
            _minimap = new MinimapPanel(canvas.transform, _view);
            _announcements = new AnnouncementPanel(canvas.transform);
            _deathRecap = new DeathRecapPanel(canvas.transform);
            _shop = new ShopPanel(canvas.transform);
            _lobby = new LobbyPanel(canvas.transform);
            _scoreboard = new ScoreboardPanel(canvas.transform);
            _scoreboard.LeaveButton.onClick.AddListener(() => GameLauncher.Leave(_view.Runner));
            _waiting = UiFactory.Text(canvas.transform, "Waiting", "Connexion à la partie…", 36, TextAnchor.MiddleCenter, Color.white);
        }

        private void Update()
        {
            MatchServices services = _view.Services;
            _waiting.gameObject.SetActive(services == null || services.Match == null);
            if (services == null || services.Match == null) return;
            BindSettings(services);
            _lobby.Bind(services);
            _hud.Bind(services);
            _shop.Bind(services);
            _lobby.Refresh(services.Match);
            _hud.Refresh(services.Match, _view.LocalChampion);
            _teamBar.Refresh(services, services.Match, _view.LocalChampion);
            _announcements.Refresh(services, services.Match, _view.LocalChampion);
            _deathRecap.Refresh(services.Match.Phase == Simulation.Match.MatchPhase.Playing ? _view.LocalChampion : null);
            _minimap.Refresh(services, services.Match.Phase != Simulation.Match.MatchPhase.Lobby && _view.LocalChampion != null);
            _shop.Refresh(services.Match.Phase == Simulation.Match.MatchPhase.Playing ? _view.LocalChampion : null);
            _scoreboard.Refresh(services, services.Match);
            _overlay.Refresh(services);
            _targetFrame.Refresh(services.View);
        }

        private void OnDestroy()
        {
            _settings?.Dispose();
            if (_controlSettings != null) _controlSettings.Changed -= RefreshKeys;
        }

        private void BindSettings(MatchServices services)
        {
            if (_settings == null)
            {
                PlayerInputCollector collector = _view.Runner.GetComponent<PlayerInputCollector>();
                if (collector == null || collector.Settings == null) return;
                _controlSettings = collector.Settings;
                _settings = new SettingsPanel(transform, _controlSettings, () => GameLauncher.Leave(_view.Runner));
                _controlSettings.Changed += RefreshKeys;
                RefreshKeys();
            }
            if (services.View.TakeMenuToggle()) _settings.Toggle();
            services.View.MenuOpen = _settings.IsOpen;
        }

        private void RefreshKeys() => _hud.SetKeyLabels(_controlSettings.KeyLabel);
    }
}
