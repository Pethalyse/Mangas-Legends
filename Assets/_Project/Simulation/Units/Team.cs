using MangaLegends.Data.Definitions;

namespace MangaLegends.Simulation.Units
{
    public enum Team : byte
    {
        None,
        Blue,
        Red,
        Neutral
    }

    public static class TeamRules
    {
        public static Team Opponent(Team team) => team == Team.Blue ? Team.Red : team == Team.Red ? Team.Blue : Team.None;

        public static bool AreEnemies(Team a, Team b)
        {
            if (a == Team.None || b == Team.None || a == b) return false;
            return true;
        }

        public static bool Matches(TeamFilter filter, Unit self, Unit other)
        {
            if (self == null || other == null) return false;
            if (self == other) return (filter & TeamFilter.Self) != 0;
            if (other.Team == Team.Neutral) return (filter & TeamFilter.Neutral) != 0;
            if (other.Team == self.Team) return (filter & TeamFilter.Allies) != 0;
            return (filter & TeamFilter.Enemies) != 0 && other.Team != Team.None;
        }
    }
}
