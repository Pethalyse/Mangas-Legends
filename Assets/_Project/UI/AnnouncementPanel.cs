using System.Collections.Generic;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // LoL announcer and kill feed, derived on each client from replicated deaths.
    public sealed class AnnouncementPanel
    {
        private const float AnnounceSeconds = 3f;
        private const float FeedSeconds = 7f;
        private const int FeedLines = 5;
        private const float MultiKillWindow = 10f;
        private static readonly string[] MultiKills = { "", "", "Double frag !", "Triple frag !", "Quadruple frag !", "Pentakill !" };
        private static readonly Color Blue = new Color(0.35f, 0.6f, 1f);
        private static readonly Color Red = new Color(1f, 0.35f, 0.32f);

        private readonly Text _announce;
        private readonly Text[] _feed = new Text[FeedLines];
        private readonly Dictionary<Unit, bool> _alive = new Dictionary<Unit, bool>();
        private readonly Dictionary<Unit, (int count, float time)> _streaks = new Dictionary<Unit, (int, float)>();
        private readonly List<(string text, float time)> _lines = new List<(string, float)>();
        private float _announceUntil;
        private bool _firstBlood;

        public AnnouncementPanel(Transform canvas)
        {
            _announce = UiFactory.Text(canvas, "Announce", "", 44, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.45f));
            UiFactory.Place(_announce, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1000f, 70f));
            _announce.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
            for (int i = 0; i < FeedLines; i++)
            {
                _feed[i] = UiFactory.Text(canvas, "Feed" + i, "", 18, TextAnchor.MiddleRight, Color.white);
                UiFactory.Place(_feed[i], new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-12f, -56f - i * 26f), new Vector2(460f, 24f));
                _feed[i].gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
            }
        }

        public void Refresh(MatchServices services, MatchController match, Unit local)
        {
            float now = Time.unscaledTime;
            if (match == null || match.Phase == MatchPhase.Lobby || local == null)
            {
                _announce.text = "";
                foreach (Text line in _feed) line.text = "";
                return;
            }

            foreach (Unit unit in services.Units.All)
            {
                if (unit == null || unit.Object == null || unit.Definition == null) continue;
                bool alive = unit.IsAlive;
                bool known = _alive.TryGetValue(unit, out bool wasAlive);
                _alive[unit] = alive;
                if (!known || !wasAlive || alive) continue;
                OnDeath(unit, local, now);
            }

            if (now > _announceUntil) _announce.text = "";
            _lines.RemoveAll(l => now - l.time > FeedSeconds);
            for (int i = 0; i < _feed.Length; i++) _feed[i].text = i < _lines.Count ? _lines[_lines.Count - 1 - i].text : "";
        }

        private void OnDeath(Unit victim, Unit local, float now)
        {
            victim.TryResolve(victim.Health.LastAttacker, out Unit killer);
            bool allyVictim = victim.Team == local.Team;
            if (victim.IsChampion && !victim.IsTrainingDummy)
            {
                Feed(killer, victim, now);
                string text = victim == local ? "Vous avez été tué"
                    : killer == local ? "Vous avez tué un ennemi"
                    : allyVictim ? "Un allié a été tué" : "Un ennemi a été tué";
                if (!_firstBlood && killer != null && killer.IsChampion)
                {
                    _firstBlood = true;
                    text = "Premier sang !";
                }
                if (killer != null && killer.IsChampion)
                {
                    int count = _streaks.TryGetValue(killer, out var streak) && now - streak.time <= MultiKillWindow ? streak.count + 1 : 1;
                    _streaks[killer] = (Mathf.Min(count, MultiKills.Length - 1), now);
                    if (count >= 2) text = MultiKills[Mathf.Min(count, MultiKills.Length - 1)];
                }
                Announce(text, now);
                return;
            }
            if (victim.Kind == UnitKind.Tower) Announce(allyVictim ? "Votre tourelle a été détruite" : "Une tourelle ennemie a été détruite", now);
            else if (MatchController.IsInhibitor(victim)) Announce(allyVictim ? "Votre inhibiteur a été détruit" : "Un inhibiteur ennemi a été détruit", now);
            else if (victim.Definition is MonsterDefinition monster && monster.IsEpic && killer != null)
            {
                bool allyKiller = killer.Team == local.Team;
                Announce($"{(allyKiller ? "Votre équipe" : "L'équipe ennemie")} a vaincu {victim.Definition.DisplayName}", now);
                Feed(killer, victim, now);
            }
        }

        private void Announce(string text, float now)
        {
            _announce.text = text;
            _announceUntil = now + AnnounceSeconds;
        }

        private void Feed(Unit killer, Unit victim, float now)
        {
            string killerName = killer != null ? Colored(killer, killer.Definition.DisplayName) : "?";
            _lines.Add(($"{killerName}  a tué  {Colored(victim, victim.Definition.DisplayName)}", now));
            if (_lines.Count > FeedLines) _lines.RemoveAt(0);
        }

        private static string Colored(Unit unit, string text)
        {
            Color color = unit.Team == Team.Blue ? Blue : unit.Team == Team.Red ? Red : new Color(0.9f, 0.8f, 0.35f);
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }
    }
}
