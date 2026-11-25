using System.Collections.Generic;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // Top-right score line and left-side ally frames of the LoL HUD.
    public sealed class TeamBarPanel
    {
        private const int MaxAllies = 4;
        private static readonly Color Blue = new Color(0.35f, 0.6f, 1f);
        private static readonly Color Red = new Color(1f, 0.35f, 0.32f);

        private readonly GameObject _score;
        private readonly Text _blueKills;
        private readonly Text _redKills;
        private readonly Text _personal;
        private readonly Text _clock;
        private readonly AllyFrame[] _allies = new AllyFrame[MaxAllies];
        private readonly List<Unit> _allyUnits = new List<Unit>();

        public TeamBarPanel(Transform canvas)
        {
            Image score = UiFactory.Box(canvas, "Score", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -8f), new Vector2(470f, 40f), new Color(0.03f, 0.04f, 0.06f, 0.9f));
            _score = score.gameObject;
            _blueKills = Cell(score.transform, 10f, 50f, Blue, TextAnchor.MiddleRight);
            Cell(score.transform, 62f, 16f, Color.white, TextAnchor.MiddleCenter).text = "-";
            _redKills = Cell(score.transform, 80f, 50f, Red, TextAnchor.MiddleLeft);
            _personal = Cell(score.transform, 140f, 240f, Color.white, TextAnchor.MiddleLeft);
            _clock = Cell(score.transform, 390f, 70f, Color.white, TextAnchor.MiddleRight);
            for (int i = 0; i < MaxAllies; i++) _allies[i] = new AllyFrame(canvas, i);
        }

        public void Refresh(MatchServices services, MatchController match, Unit local)
        {
            bool visible = match != null && match.Phase != MatchPhase.Lobby && local != null;
            _score.SetActive(visible);
            if (!visible)
            {
                foreach (AllyFrame frame in _allies) frame.Hide();
                return;
            }

            int blue = 0, red = 0;
            _allyUnits.Clear();
            foreach (Unit unit in services.Units.All)
            {
                if (unit == null || unit.Object == null || !unit.IsChampion) continue;
                ChampionProgress progress = unit.GetComponent<ChampionProgress>();
                if (unit.Team == Team.Blue) blue += progress.Kills;
                else if (unit.Team == Team.Red) red += progress.Kills;
                if (unit != local && unit.Team == local.Team) _allyUnits.Add(unit);
            }
            _blueKills.text = blue.ToString();
            _redKills.text = red.ToString();
            ChampionProgress mine = local.GetComponent<ChampionProgress>();
            _personal.text = $"{mine.Kills}/{mine.Deaths}/{mine.Assists}     CS {mine.MinionKills}";
            int seconds = Mathf.FloorToInt(match.ElapsedSeconds);
            _clock.text = $"{seconds / 60:00}:{seconds % 60:00}";

            _allyUnits.Sort((a, b) => a.SortKey.CompareTo(b.SortKey));
            for (int i = 0; i < _allies.Length; i++)
            {
                if (i < _allyUnits.Count) _allies[i].Show(_allyUnits[i]);
                else _allies[i].Hide();
            }
        }

        private static Text Cell(Transform parent, float x, float width, Color color, TextAnchor anchor)
        {
            Text text = UiFactory.Text(parent, "Cell", "", 22, anchor, color);
            UiFactory.Place(text, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(width, 36f));
            return text;
        }

        private sealed class AllyFrame
        {
            private readonly GameObject _root;
            private readonly Text _name;
            private readonly Text _level;
            private readonly Image _health;
            private readonly Text _respawn;

            public AllyFrame(Transform canvas, int index)
            {
                Image frame = UiFactory.Box(canvas, "Ally" + index, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 120f - index * 66f), new Vector2(200f, 58f), new Color(0.03f, 0.04f, 0.06f, 0.85f));
                _root = frame.gameObject;
                Image portrait = UiFactory.Box(frame.transform, "Portrait", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(50f, 50f), new Color(0.15f, 0.15f, 0.2f));
                _level = UiFactory.Text(portrait.transform, "Level", "", 18, TextAnchor.LowerRight, new Color(0.78f, 0.67f, 0.43f));
                _respawn = UiFactory.Text(portrait.transform, "Respawn", "", 22, TextAnchor.MiddleCenter, Color.white);
                _name = UiFactory.Text(frame.transform, "Name", "", 15, TextAnchor.UpperLeft, Color.white);
                UiFactory.Place(_name, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -4f), new Vector2(136f, 22f));
                UiFactory.Bar(frame.transform, "Health", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(60f, 8f), new Vector2(134f, 12f), UiFactory.Health, out _health);
                _root.SetActive(false);
            }

            public void Show(Unit unit)
            {
                _root.SetActive(true);
                ChampionProgress progress = unit.GetComponent<ChampionProgress>();
                string player = progress.PlayerName.ToString();
                _name.text = string.IsNullOrEmpty(player) ? unit.Definition.DisplayName : $"{player}\n<size=12>{unit.Definition.DisplayName}</size>";
                _level.text = unit.Stats.Level.ToString();
                UiFactory.SetFill(_health, unit.IsAlive ? unit.Health.Ratio : 0f);
                _respawn.text = unit.IsAlive ? "" : Mathf.CeilToInt(progress.RespawnRemaining).ToString();
            }

            public void Hide() => _root.SetActive(false);
        }
    }
}
