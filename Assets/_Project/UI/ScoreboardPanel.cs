using System.Text;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    public sealed class ScoreboardPanel
    {
        private readonly GameObject _root;
        private readonly Text _title;
        private readonly Text _blue;
        private readonly Text _red;
        private readonly GameObject _leave;

        public ScoreboardPanel(Transform canvas)
        {
            Image panel = UiFactory.Box(canvas, "Scoreboard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1300f, 620f), UiFactory.Panel);
            _root = panel.gameObject;
            _title = UiFactory.Text(panel.transform, "Title", "", 40, TextAnchor.UpperCenter, UiFactory.Accent);
            _title.rectTransform.offsetMax = new Vector2(0f, -14f);
            _blue = Column(panel.transform, -320f, new Color(0.5f, 0.7f, 1f));
            _red = Column(panel.transform, 320f, new Color(1f, 0.6f, 0.55f));
            Button leave = UiFactory.Button(panel.transform, "Leave", "Retour au menu", () => { }, 26);
            UiFactory.Place(leave, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(320f, 56f));
            _leave = leave.gameObject;
            _root.SetActive(false);
        }

        public Button LeaveButton => _leave.GetComponent<Button>();

        public void Refresh(MatchServices services, MatchController match)
        {
            bool ended = match != null && match.Phase == MatchPhase.Ended;
            bool visible = match != null && match.Phase != MatchPhase.Lobby && (ended || services.View.ScoreboardHeld);
            _root.SetActive(visible);
            if (!visible) return;
            _leave.SetActive(ended);
            Unit local = services.View.LocalChampion;
            if (ended)
            {
                bool won = local != null && local.Team == match.Winner;
                _title.text = local == null ? $"Victoire de l'équipe {Name(match.Winner)}" : won ? "VICTOIRE" : "DÉFAITE";
            }
            else _title.text = "Tableau des scores";
            _blue.text = Roster(services, Team.Blue);
            _red.text = Roster(services, Team.Red);
        }

        private static Text Column(Transform root, float x, Color color)
        {
            Text text = UiFactory.Text(root, "Column", "", 24, TextAnchor.UpperLeft, color);
            UiFactory.Place(text, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, -90f), new Vector2(600f, 420f));
            return text;
        }

        private static string Roster(MatchServices services, Team team)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Équipe {Name(team)}");
            builder.AppendLine();
            foreach (Unit unit in services.Units.All)
            {
                if (unit == null || !unit.IsChampion || unit.Team != team) continue;
                ChampionProgress progress = unit.GetComponent<ChampionProgress>();
                builder.AppendLine($"{progress.PlayerName} ({unit.Definition.DisplayName}) niv. {unit.Stats.Level}");
                builder.AppendLine($"   {progress.Kills}/{progress.Deaths}/{progress.Assists}   CS {progress.MinionKills}   {progress.TotalGold} or");
            }
            return builder.ToString();
        }

        private static string Name(Team team) => team == Team.Blue ? "bleue" : team == Team.Red ? "rouge" : "—";
    }
}
