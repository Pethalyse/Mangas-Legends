using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MangaLegends.Networking
{
    public sealed class GameLauncher : MonoBehaviour
    {
        private const string NameKey = "mangalegends.player_name";
        private const string CodeKey = "mangalegends.session_code";

        [SerializeField] private NetworkRunner _runnerPrefab;
        [SerializeField] private string _matchScene = "Match";

        private NetworkRunner _runner;

        public bool IsBusy { get; private set; }
        public string LastError { get; private set; }
        public static string SavedPlayerName => PlayerPrefs.GetString(NameKey, "Joueur");

        public string PlayerName
        {
            get => SavedPlayerName;
            set => PlayerPrefs.SetString(NameKey, value);
        }
        public string LastSessionCode
        {
            get => PlayerPrefs.GetString(CodeKey, NewSessionCode());
            set => PlayerPrefs.SetString(CodeKey, value);
        }

        public event Action StateChanged;

        public Task Host(string sessionCode) => Launch(GameMode.Host, sessionCode);
        public Task Join(string sessionCode) => Launch(GameMode.Client, sessionCode);
        public Task Solo() => Launch(GameMode.Single, NewSessionCode());

        public static string NewSessionCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] code = new char[5];
            System.Random random = new System.Random();
            for (int i = 0; i < code.Length; i++) code[i] = alphabet[random.Next(alphabet.Length)];
            return new string(code);
        }

        private async Task Launch(GameMode mode, string sessionCode)
        {
            if (IsBusy) return;
            IsBusy = true;
            LastError = null;
            StateChanged?.Invoke();
            sessionCode = string.IsNullOrWhiteSpace(sessionCode) ? NewSessionCode() : sessionCode.Trim().ToUpperInvariant();
            LastSessionCode = sessionCode;

            _runner = Instantiate(_runnerPrefab);
            _runner.name = $"Runner ({mode})";
            DontDestroyOnLoad(_runner.gameObject);
            _runner.ProvideInput = true;

            int sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Assets/_Project/Scenes/{_matchScene}.unity");
            NetworkSceneInfo scene = new NetworkSceneInfo();
            if (mode != GameMode.Client) scene.AddSceneRef(SceneRef.FromIndex(sceneIndex), LoadSceneMode.Single);
            StartGameResult result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = sessionCode,
                PlayerCount = 10,
                Scene = scene,
                SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
            });

            IsBusy = false;
            if (!result.Ok)
            {
                LastError = result.ShutdownReason.ToString();
                if (_runner != null) Destroy(_runner.gameObject);
                _runner = null;
            }
            StateChanged?.Invoke();
        }

        public static async void Leave(NetworkRunner runner, string menuScene = "MainMenu")
        {
            if (runner != null) await runner.Shutdown();
            SceneManager.LoadScene(menuScene);
        }
    }
}
