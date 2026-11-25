using System.Collections.Generic;
using MangaLegends.Core.Simulation;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Units;

namespace MangaLegends.Simulation.Match
{
    public sealed class MatchRewards
    {
        private readonly MatchController _match;
        private readonly List<Unit> _nearby = new List<Unit>();
        private readonly List<Unit> _assisters = new List<Unit>();

        public MatchRewards(MatchController match) => _match = match;

        private GameRulesConfig Rules => _match.Rules;

        public void Grant(Unit victim, Unit killer)
        {
            Unit killingChampion = killer != null && killer.IsChampion ? killer : null;
            if (victim.IsChampion)
            {
                GrantChampionKill(victim, killingChampion);
                return;
            }

            UnitDefinition definition = victim.Definition;
            if (definition == null) return;
            Team rewarded = killer != null && killer.Team != Team.Neutral ? killer.Team : TeamRules.Opponent(victim.Team);

            if (victim.IsStructure)
            {
                foreach (Unit champion in ChampionsNear(victim, rewarded)) Progress(champion).AddGold(definition.GoldBounty);
                return;
            }

            if (killingChampion != null)
            {
                ChampionProgress progress = Progress(killingChampion);
                progress.AddGold(definition.GoldBounty);
                progress.MinionKills++;
                if (victim.IsMonster) GrantMonster(victim, killingChampion);
            }
            ShareExperience(victim, rewarded, definition.ExperienceBounty);
        }

        private void GrantChampionKill(Unit victim, Unit killer)
        {
            ChampionProgress victimProgress = Progress(victim);
            victimProgress.Deaths++;
            victimProgress.KillStreak = 0;

            int gold = Rules.ChampionKillGold;
            CollectAssisters(victim, killer);
            if (killer != null)
            {
                if (_match.FirstBlood()) gold += Rules.FirstBloodBonusGold;
                ChampionProgress progress = Progress(killer);
                progress.Kills++;
                progress.KillStreak++;
                progress.AddGold(gold);
            }
            if (_assisters.Count > 0)
            {
                float share = gold * Rules.AssistGoldShare / _assisters.Count;
                foreach (Unit assister in _assisters)
                {
                    ChampionProgress progress = Progress(assister);
                    progress.Assists++;
                    progress.AddGold(share);
                }
            }
            int experience = Rules.KillExperienceBase + Rules.KillExperiencePerLevel * victim.Stats.Level;
            ShareExperience(victim, TeamRules.Opponent(victim.Team), experience);
        }

        private void GrantMonster(Unit monster, Unit killer)
        {
            killer.Health.Heal(Rules.MonsterKillHeal(killer.Stats.Level), killer);
            if (!(monster.Definition is MonsterDefinition definition)) return;
            if (definition.KillerReward != null)
                killer.Status.Apply(new StatusApplication(definition.KillerReward, killer, 1, 1, 0f, null));
            if (definition.TeamReward == null && definition.TeamGold <= 0) return;
            foreach (Unit unit in _match.Services.Units.All)
            {
                if (unit == null || !unit.IsChampion || unit.Team != killer.Team) continue;
                if (definition.TeamReward != null && unit.IsAlive) unit.Status.Apply(new StatusApplication(definition.TeamReward, killer, 1, 1, 0f, null));
                if (definition.TeamGold > 0) Progress(unit).AddGold(definition.TeamGold);
            }
        }

        private void ShareExperience(Unit victim, Team team, int amount)
        {
            if (amount <= 0) return;
            List<Unit> receivers = ChampionsNear(victim, team);
            if (receivers.Count == 0) return;
            int each = (int)(amount * Rules.ExperienceShareRatio(receivers.Count));
            foreach (Unit champion in receivers) Progress(champion).AddExperience(each);
        }

        private List<Unit> ChampionsNear(Unit victim, Team team)
        {
            _nearby.Clear();
            foreach (Unit unit in _match.Services.Units.All)
            {
                if (unit == null || !unit.IsChampion || !unit.IsAlive || unit.Team != team) continue;
                if (Services.UnitRegistry.FlatDistance(unit.Position, victim.Position) <= Rules.ExperienceShareRadius) _nearby.Add(unit);
            }
            _nearby.Sort((a, b) => a.SortKey.CompareTo(b.SortKey));
            return _nearby;
        }

        private void CollectAssisters(Unit victim, Unit killer)
        {
            _assisters.Clear();
            int window = TickTime.SecondsToTicks(Rules.AssistWindowSeconds, victim.TickRate);
            Health health = victim.Health;
            for (int i = 0; i < Health.HitHistory; i++)
            {
                HitRecord hit = health.Hits[i];
                if (hit.IsHeal || victim.Tick - hit.Tick > window || !victim.TryResolve(hit.Source, out Unit source)) continue;
                if (!source.IsChampion || source == killer || source.Team == victim.Team || _assisters.Contains(source)) continue;
                _assisters.Add(source);
            }
        }

        private static ChampionProgress Progress(Unit champion) => champion.GetComponent<ChampionProgress>();
    }
}
