using MangaLegends.Input;
using MangaLegends.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    public sealed class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameLauncher _launcher;
        [SerializeField] private UnityEngine.InputSystem.InputActionAsset _controls;

        private InputField _name;
        private InputField _code;
        private Text _status;
        private Button[] _buttons;
        private SettingsPanel _settings;

        private void Start()
        {
            Canvas canvas = UiFactory.Canvas(transform, "MainMenu", 0);
            UiFactory.Image(canvas.transform, "Background", new Color(0.04f, 0.05f, 0.08f));
            Image panel = UiFactory.Box(canvas.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 690f), UiFactory.Panel);
            Transform root = panel.transform;

            Text title = UiFactory.Text(root, "Title", "MANGA LEGENDS", 64, TextAnchor.MiddleCenter, UiFactory.Accent);
            UiFactory.Place(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(680f, 90f));

            _name = Field(root, "Pseudo", _launcher.PlayerName, -150f);
            _code = Field(root, "Code de la partie", _launcher.LastSessionCode, -240f);

            _buttons = new[]
            {
                MenuButton(root, "Héberger", () => Run(() => _launcher.Host(_code.text)), -340f),
                MenuButton(root, "Rejoindre", () => Run(() => _launcher.Join(_code.text)), -410f),
                MenuButton(root, "Entraînement (solo)", () => Run(_launcher.Solo), -480f),
                MenuButton(root, "Options", () => _settings?.Show(), -550f)
            };
            if (_controls != null) _settings = new SettingsPanel(transform, new ControlSettings(_controls), null);

            _status = UiFactory.Text(root, "Status", "", 22, TextAnchor.MiddleCenter, new Color(1f, 0.6f, 0.5f));
            UiFactory.Place(_status, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(680f, 40f));
            _launcher.StateChanged += Refresh;
        }

        private void OnDestroy()
        {
            if (_launcher != null) _launcher.StateChanged -= Refresh;
            _settings?.Dispose();
        }

        private InputField Field(Transform root, string label, string value, float y)
        {
            Text caption = UiFactory.Text(root, label, label, 22, TextAnchor.LowerLeft, Color.white);
            UiFactory.Place(caption, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y + 34f), new Vector2(560f, 30f));
            InputField field = UiFactory.Input(root, label + " Field", value, label);
            UiFactory.Place(field, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(560f, 50f));
            return field;
        }

        private static Button MenuButton(Transform root, string label, System.Action onClick, float y)
        {
            Button button = UiFactory.Button(root, label, label, onClick, 26);
            UiFactory.Place(button, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(560f, 56f));
            return button;
        }

        private void Run(System.Func<System.Threading.Tasks.Task> action)
        {
            _launcher.PlayerName = string.IsNullOrWhiteSpace(_name.text) ? "Joueur" : _name.text.Trim();
            _ = action();
            Refresh();
        }

        private void Refresh()
        {
            if (_status == null) return;
            foreach (Button button in _buttons) button.interactable = !_launcher.IsBusy;
            _status.text = _launcher.IsBusy ? "Connexion…" : string.IsNullOrEmpty(_launcher.LastError) ? "" : $"Échec : {_launcher.LastError}";
        }
    }
}
