using System.Collections.Generic;
using Fusion;
using MangaLegends.Data.Definitions;
using MangaLegends.Networking;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // LoL-style champion select: the two teams on the sides, the champion grid with a search and a random tile in the
    // middle, "Lock in", and the two summoner spells below; clicking a spell opens the summoner spell picker.
    public sealed class LobbyPanel
    {
        private const float Portrait = 92f;
        private static readonly Color Gold = new Color(0.78f, 0.67f, 0.43f);
        private static readonly Color Back = new Color(0.02f, 0.05f, 0.08f, 1f);

        private readonly GameObject _root;
        private readonly Text _title;
        private readonly Text _blue;
        private readonly Text _red;
        private readonly RectTransform _grid;
        private readonly InputField _search;
        private readonly Button _lock;
        private readonly Text _lockLabel;
        private readonly Button _start;
        private readonly Image _spell1;
        private readonly Image _spell2;
        private readonly Text _spell1Label;
        private readonly Text _spell2Label;
        private readonly GameObject _picker;
        private readonly Text _pickerTitle;
        private readonly List<(short index, Image frame)> _portraits = new List<(short, Image)>();
        private MatchServices _services;
        private bool _named;
        private string _builtFilter;
        private int _pickingSpell = -1;

        public LobbyPanel(Transform canvas)
        {
            Image border = UiFactory.Box(canvas, "Lobby", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 860f), Gold);
            _root = border.gameObject;
            Image panel = UiFactory.Image(border.transform, "Back", Back);
            panel.rectTransform.offsetMin = new Vector2(2f, 2f);
            panel.rectTransform.offsetMax = new Vector2(-2f, -2f);
            Transform root = panel.transform;

            _title = UiFactory.Text(root, "Title", "CHOISISSEZ VOTRE CHAMPION", 34, TextAnchor.MiddleCenter, new Color(0.94f, 0.9f, 0.8f));
            UiFactory.Place(_title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(900f, 50f));

            _blue = TeamColumn(root, "ÉQUIPE BLEUE", new Vector2(20f, -90f), new Vector2(0f, 1f), new Color(0.5f, 0.7f, 1f), () => Choose(team: Team.Blue));
            _red = TeamColumn(root, "ÉQUIPE ROUGE", new Vector2(-20f, -90f), new Vector2(1f, 1f), new Color(1f, 0.6f, 0.55f), () => Choose(team: Team.Red));

            _search = UiFactory.Input(root, "Search", "", "Rechercher un champion…");
            UiFactory.Place(_search, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(420f, 36f));
            _grid = UiFactory.Rect(root, "Grid", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-400f, -640f), new Vector2(400f, -130f));

            _lock = UiFactory.Button(root, "Lock", "VERROUILLER", Lock, 26);
            UiFactory.Place(_lock, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(300f, 56f));
            _lockLabel = _lock.GetComponentInChildren<Text>();

            _spell1 = SpellSlot(root, new Vector2(-50f, 30f), 0, out _spell1Label);
            _spell2 = SpellSlot(root, new Vector2(50f, 30f), 1, out _spell2Label);

            _start = UiFactory.Button(root, "Start", "LANCER LA PARTIE", Start, 22);
            UiFactory.Place(_start, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 20f), new Vector2(280f, 54f));
            Button leave = UiFactory.Button(root, "Leave", "QUITTER", Leave, 20);
            UiFactory.Place(leave, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(200f, 54f));

            Image picker = UiFactory.Box(root, "SpellPicker", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(760f, 330f), Gold);
            _picker = picker.gameObject;
            Image pickerBack = UiFactory.Image(picker.transform, "Back", new Color(0.03f, 0.07f, 0.1f, 0.99f));
            pickerBack.rectTransform.offsetMin = new Vector2(2f, 2f);
            pickerBack.rectTransform.offsetMax = new Vector2(-2f, -2f);
            _pickerTitle = UiFactory.Text(pickerBack.transform, "Title", "SORTS D'INVOCATEUR", 20, TextAnchor.UpperCenter, Gold);
            _pickerTitle.rectTransform.offsetMax = new Vector2(0f, -10f);
            _picker.SetActive(false);
        }

        public void Bind(MatchServices services)
        {
            if (_services == services) return;
            _services = services;
            BuildSpellPicker();
        }

        public void Refresh(MatchController match)
        {
            bool visible = match != null && match.Phase == MatchPhase.Lobby;
            _root.SetActive(visible);
            if (!visible || _services == null) return;
            if (_builtFilter != _search.text) BuildGrid(_search.text);

            PlayerSlot local = LocalSlot(match);
            if (local != null && !_named)
            {
                _named = true;
                local.RpcChoose(local.Champion, local.Summoner1, local.Summoner2, local.Team, GameLauncher.SavedPlayerName);
            }
            string code = match.Runner.SessionInfo != null && match.Runner.SessionInfo.IsValid ? match.Runner.SessionInfo.Name : "";
            _title.text = string.IsNullOrEmpty(code) ? "CHOISISSEZ VOTRE CHAMPION" : $"CHOISISSEZ VOTRE CHAMPION — code {code}";
            _blue.text = Roster(match, Team.Blue);
            _red.text = Roster(match, Team.Red);
            _start.gameObject.SetActive(match.Runner.IsServer);
            if (local == null) return;
            foreach (var portrait in _portraits)
                portrait.frame.color = portrait.index == local.Champion ? Gold : new Color(0.15f, 0.18f, 0.24f);
            _lockLabel.text = local.Ready ? "VERROUILLÉ" : "VERROUILLER";
            _lock.interactable = !local.Ready;
            ShowSpell(_spell1, _spell1Label, local.Summoner1, "D");
            ShowSpell(_spell2, _spell2Label, local.Summoner2, "F");
        }

        private Text TeamColumn(Transform root, string title, Vector2 position, Vector2 anchor, Color color, System.Action join)
        {
            Image box = UiFactory.Box(root, title, anchor, anchor, position, new Vector2(300f, 560f), new Color(0f, 0f, 0f, 0.35f));
            Text header = UiFactory.Text(box.transform, "Header", title, 22, TextAnchor.UpperCenter, color);
            header.rectTransform.offsetMax = new Vector2(0f, -10f);
            Text list = UiFactory.Text(box.transform, "List", "", 20, TextAnchor.UpperLeft, Color.white);
            list.rectTransform.offsetMin = new Vector2(16f, 60f);
            list.rectTransform.offsetMax = new Vector2(-16f, -50f);
            Button button = UiFactory.Button(box.transform, "Join", "Rejoindre", join, 18);
            UiFactory.Place(button, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(200f, 40f));
            return list;
        }

        private Image SpellSlot(Transform root, Vector2 position, int which, out Text label)
        {
            Image frame = UiFactory.Box(root, "Spell" + which, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), position, new Vector2(80f, 80f), new Color(0.15f, 0.18f, 0.24f));
            frame.gameObject.AddComponent<Button>().onClick.AddListener(() => OpenPicker(which));
            label = UiFactory.Text(frame.transform, "Name", "", 13, TextAnchor.MiddleCenter, Color.white);
            label.raycastTarget = false;
            UiFactory.Border(frame, Gold, 2f);
            return frame;
        }

        private void ShowSpell(Image frame, Text label, short index, string key)
        {
            var spells = _services.Content.SummonerSpells;
            SummonerSpellDefinition spell = index >= 0 && index < spells.Count ? spells[index] : null;
            frame.sprite = spell != null ? spell.Icon : null;
            frame.color = spell != null && spell.Icon != null ? Color.white : new Color(0.15f, 0.18f, 0.24f);
            label.text = spell != null && spell.Icon == null ? $"{spell.DisplayName}\n[{key}]" : $"[{key}]";
        }

        private void BuildGrid(string filter)
        {
            _builtFilter = filter;
            foreach (Transform child in _grid) Object.Destroy(child.gameObject);
            _portraits.Clear();
            var champions = _services.Content.Champions;
            int perRow = Mathf.FloorToInt(800f / (Portrait + 16f));
            int cell = 0;
            AddPortrait(cell++, perRow, -1, "?", "Aléatoire", new Color(0.12f, 0.14f, 0.2f), null);
            for (int i = 0; i < champions.Count; i++)
            {
                ChampionDefinition champion = champions[i];
                if (!champion.Selectable) continue;
                if (!string.IsNullOrWhiteSpace(filter) && !(champion.DisplayName ?? "").ToLowerInvariant().Contains(filter.Trim().ToLowerInvariant())) continue;
                AddPortrait(cell++, perRow, (short)i, Initials(champion.DisplayName), champion.DisplayName, champion.Tint, champion.Icon);
            }
        }

        private void AddPortrait(int cell, int perRow, short index, string initials, string name, Color tint, Sprite icon)
        {
            Vector2 position = new Vector2((cell % perRow) * (Portrait + 16f), -(cell / perRow) * (Portrait + 34f));
            Image frame = UiFactory.Box(_grid, name, new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(Portrait, Portrait), new Color(0.15f, 0.18f, 0.24f));
            Image picture = UiFactory.Image(frame.transform, "Picture", icon != null ? Color.white : Color.Lerp(tint, Color.black, 0.35f));
            picture.rectTransform.offsetMin = new Vector2(3f, 3f);
            picture.rectTransform.offsetMax = new Vector2(-3f, -3f);
            picture.sprite = icon;
            picture.raycastTarget = false;
            if (icon == null)
            {
                Text letters = UiFactory.Text(frame.transform, "Initials", initials, 30, TextAnchor.MiddleCenter, Color.white);
                letters.raycastTarget = false;
            }
            Text caption = UiFactory.Text(frame.transform, "Name", name, 14, TextAnchor.UpperCenter, new Color(0.9f, 0.88f, 0.8f));
            UiFactory.Place(caption, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(Portrait + 14f, 30f));
            caption.raycastTarget = false;
            short chosen = index;
            frame.gameObject.AddComponent<Button>().onClick.AddListener(() => Choose(champion: chosen >= 0 ? chosen : RandomChampion()));
            _portraits.Add((index, frame));
        }

        private void BuildSpellPicker()
        {
            var spells = _services.Content.SummonerSpells;
            Transform root = _picker.transform.GetChild(0);
            for (int i = 0; i < spells.Count; i++)
            {
                short index = (short)i;
                Vector2 position = new Vector2(24f + (i % 6) * 120f, -50f - (i / 6) * 130f);
                Image frame = UiFactory.Box(root, spells[i].Id, new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(80f, 80f), new Color(0.15f, 0.18f, 0.24f));
                frame.sprite = spells[i].Icon;
                if (spells[i].Icon != null) frame.color = Color.white;
                Text name = UiFactory.Text(frame.transform, "Name", spells[i].DisplayName, 14, TextAnchor.UpperCenter, Color.white);
                UiFactory.Place(name, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(116f, 40f));
                name.raycastTarget = false;
                frame.gameObject.AddComponent<Button>().onClick.AddListener(() => PickSpell(index));
            }
        }

        private void OpenPicker(int which)
        {
            _pickingSpell = which;
            _pickerTitle.text = which == 0 ? "SORT D'INVOCATEUR (D)" : "SORT D'INVOCATEUR (F)";
            _picker.SetActive(true);
        }

        private void PickSpell(short index)
        {
            PlayerSlot local = LocalSlot(_services?.Match);
            _picker.SetActive(false);
            if (local == null || _pickingSpell < 0) return;
            short first = _pickingSpell == 0 ? index : local.Summoner1;
            short second = _pickingSpell == 1 ? index : local.Summoner2;
            if (first == second)
            {
                if (_pickingSpell == 0) second = local.Summoner1;
                else first = local.Summoner2;
            }
            local.RpcChoose(local.Champion, first, second, local.Team, local.PlayerName);
            _pickingSpell = -1;
        }

        private short RandomChampion()
        {
            var champions = _services.Content.Champions;
            for (int attempt = 0; attempt < 32; attempt++)
            {
                short pick = (short)Random.Range(0, champions.Count);
                if (champions[pick].Selectable) return pick;
            }
            return 0;
        }

        private void Choose(short champion = -1, Team team = Team.None)
        {
            PlayerSlot local = LocalSlot(_services?.Match);
            if (local == null || (local.Ready && champion >= 0)) return;
            local.RpcChoose(champion >= 0 ? champion : local.Champion, local.Summoner1, local.Summoner2, team != Team.None ? team : local.Team, local.PlayerName);
        }

        private void Lock()
        {
            PlayerSlot local = LocalSlot(_services?.Match);
            if (local != null) local.RpcSetReady(true);
        }

        private void Start()
        {
            MatchController match = _services?.Match;
            if (match != null && match.Runner.IsServer) match.StartMatch();
        }

        private void Leave() => GameLauncher.Leave(_services?.Match != null ? _services.Match.Runner : null);

        private string Roster(MatchController match, Team team)
        {
            var builder = new System.Text.StringBuilder();
            var champions = _services.Content.Champions;
            foreach (PlayerSlot slot in match.Slots)
            {
                if (slot.Team != team) continue;
                string champion = slot.Champion >= 0 && slot.Champion < champions.Count ? champions[slot.Champion].DisplayName : "…";
                builder.AppendLine($"{slot.PlayerName}{(slot.IsLocal ? " (vous)" : "")}");
                builder.AppendLine($"   {champion}{(slot.Ready ? "  ✔" : "")}");
            }
            return builder.ToString();
        }

        private static string Initials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?";
            string[] words = name.Split(' ');
            return words.Length > 1 ? $"{words[0][0]}{words[1][0]}" : name.Substring(0, Mathf.Min(2, name.Length));
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
