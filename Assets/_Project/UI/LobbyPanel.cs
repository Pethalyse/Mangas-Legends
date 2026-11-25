using System.Text;
using Fusion;
using MangaLegends.Networking;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    public sealed class LobbyPanel
    {
        private readonly GameObject _root;
        private readonly Text _title;
        private readonly Text _blue;
        private readonly Text _red;
        private readonly Text _spell1;
        private readonly Text _spell2;
        private readonly Button _start;
        private MatchServices _services;
        private bool _named;

        public LobbyPanel(Transform canvas)
        {
            Image panel = UiFactory.Box(canvas, "Lobby", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 760f), UiFactory.Panel);
            _root = panel.gameObject;
            Transform root = panel.transform;
            _title = UiFactory.Text(root, "Title", "Salon", 40, TextAnchor.MiddleCenter, UiFactory.Accent);
            UiFactory.Place(_title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1100f, 60f));

            _blue = Column(root, "Équipe bleue", new Vector2(-290f, -100f), new Color(0.5f, 0.7f, 1f));
            _red = Column(root, "Équipe rouge", new Vector2(290f, -100f), new Color(1f, 0.6f, 0.55f));

            Place(UiFactory.Button(root, "Blue", "Rejoindre Bleu", () => Choose(team: Team.Blue)), new Vector2(-290f, -400f), new Vector2(300f, 48f));
            Place(UiFactory.Button(root, "Red", "Rejoindre Rouge", () => Choose(team: Team.Red)), new Vector2(290f, -400f), new Vector2(300f, 48f));

            _spell1 = Place(UiFactory.Button(root, "Spell1", "Sort 1", () => CycleSpell(0)), new Vector2(-290f, -470f), new Vector2(300f, 48f)).GetComponentInChildren<Text>();
            _spell2 = Place(UiFactory.Button(root, "Spell2", "Sort 2", () => CycleSpell(1)), new Vector2(290f, -470f), new Vector2(300f, 48f)).GetComponentInChildren<Text>();

            _start = Place(UiFactory.Button(root, "Start", "Lancer la partie", Start, 28), new Vector2(150f, -690f), new Vector2(360f, 60f));
            Place(UiFactory.Button(root, "Leave", "Quitter", Leave), new Vector2(-250f, -690f), new Vector2(240f, 60f));
        }

        public void Bind(MatchServices services)
        {
            if (_services == services) return;
            _services = services;
            BuildChampionButtons();
        }

        public void Refresh(MatchController match)
        {
            bool visible = match != null && match.Phase == MatchPhase.Lobby;
            _root.SetActive(visible);
            if (!visible || _services == null) return;

            PlayerSlot local = LocalSlot(match);
            if (local != null && !_named)
            {
                _named = true;
                local.RpcChoose(local.Champion, local.Summoner1, local.Summoner2, local.Team, GameLauncher.SavedPlayerName);
            }
            string code = match.Runner.SessionInfo != null && match.Runner.SessionInfo.IsValid ? match.Runner.SessionInfo.Name : "";
            _title.text = $"Salon — code : {code}";
            _blue.text = Roster(match, Team.Blue);
            _red.text = Roster(match, Team.Red);
            _start.gameObject.SetActive(match.Runner.IsServer);
            if (local == null) return;
            var spells = _services.Content.SummonerSpells;
            _spell1.text = "Sort D : " + (local.Summoner1 >= 0 && local.Summoner1 < spells.Count ? spells[local.Summoner1].DisplayName : "—");
            _spell2.text = "Sort F : " + (local.Summoner2 >= 0 && local.Summoner2 < spells.Count ? spells[local.Summoner2].DisplayName : "—");
        }

        private Text Column(Transform root, string title, Vector2 position, Color color)
        {
            Image box = UiFactory.Box(root, title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, new Vector2(520f, 270f), new Color(0f, 0f, 0f, 0.35f));
            Text header = UiFactory.Text(box.transform, "Header", title, 28, TextAnchor.UpperCenter, color);
            header.rectTransform.offsetMax = new Vector2(0f, -8f);
            Text list = UiFactory.Text(box.transform, "List", "", 24, TextAnchor.UpperLeft, Color.white);
            list.rectTransform.offsetMin = new Vector2(20f, 10f);
            list.rectTransform.offsetMax = new Vector2(-20f, -50f);
            return list;
        }

        private static T Place<T>(T component, Vector2 position, Vector2 size) where T : Component
        {
            UiFactory.Place(component, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, size);
            return component;
        }

        private void BuildChampionButtons()
        {
            var champions = _services.Content.Champions;
            Transform root = _root.transform;
            float width = 180f;
            for (int i = 0; i < champions.Count; i++)
            {
                short index = (short)i;
                Button button = UiFactory.Button(root, champions[i].DisplayName, champions[i].DisplayName, () => Choose(champion: index), 20);
                float x = (i - (champions.Count - 1) * 0.5f) * (width + 12f);
                Place(button, new Vector2(x, -560f), new Vector2(width, 70f));
                if (champions[i].Icon != null)
                {
                    Image icon = UiFactory.Box(button.transform, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(60f, 60f), Color.white);
                    icon.sprite = champions[i].Icon;
                }
            }
        }

        private void Choose(short champion = -1, Team team = Team.None)
        {
            PlayerSlot local = LocalSlot(_services?.Match);
            if (local == null) return;
            local.RpcChoose(champion >= 0 ? champion : local.Champion, local.Summoner1, local.Summoner2, team != Team.None ? team : local.Team, local.PlayerName);
        }

        private void CycleSpell(int which)
        {
            PlayerSlot local = LocalSlot(_services?.Match);
            if (local == null) return;
            int count = _services.Content.SummonerSpells.Count;
            if (count == 0) return;
            short current = which == 0 ? local.Summoner1 : local.Summoner2;
            short other = which == 0 ? local.Summoner2 : local.Summoner1;
            short next = (short)((current + 1) % count);
            if (next == other) next = (short)((next + 1) % count);
            local.RpcChoose(local.Champion, which == 0 ? next : local.Summoner1, which == 1 ? next : local.Summoner2, local.Team, local.PlayerName);
        }

        private void Start()
        {
            MatchController match = _services?.Match;
            if (match != null && match.Runner.IsServer) match.StartMatch();
        }

        private void Leave() => GameLauncher.Leave(_services?.Match != null ? _services.Match.Runner : null);

        private string Roster(MatchController match, Team team)
        {
            StringBuilder builder = new StringBuilder();
            var champions = _services.Content.Champions;
            foreach (PlayerSlot slot in match.Slots)
            {
                if (slot.Team != team) continue;
                string champion = slot.Champion >= 0 && slot.Champion < champions.Count ? champions[slot.Champion].DisplayName : "?";
                string you = slot.IsLocal ? " (vous)" : "";
                builder.AppendLine($"{slot.PlayerName}{you} — {champion}");
            }
            return builder.ToString();
        }

        private static PlayerSlot LocalSlot(MatchController match)
        {
            if (match == null) return null;
            foreach (PlayerSlot slot in match.Slots)
                if (slot.IsLocal) return slot;
            return null;
        }
    }
}
