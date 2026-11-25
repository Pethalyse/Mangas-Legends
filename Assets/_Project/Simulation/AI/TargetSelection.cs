using MangaLegends.Data.Definitions;
using MangaLegends.Core.Simulation;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.AI
{
    public static class TargetSelection
    {
        private const float HarassMemorySeconds = 2f;

        public static bool IsValidEnemy(Unit self, Unit other) =>
            other != null && other.IsAlive && other.IsTargetable && !other.IsInvulnerable && other.Kind != UnitKind.Ward && self.IsEnemyOf(other);

        // Genre rule: an enemy champion that hurt an allied champion nearby is punished first, then the closest non-champion.
        public static Unit Pick(Unit self, float radius)
        {
            UnitRegistry units = self.Services.Units;
            int memory = TickTime.SecondsToTicks(HarassMemorySeconds, self.TickRate);
            Unit harasser = units.Closest(self.Position, radius, u => IsValidEnemy(self, u) && u.IsChampion && u.Health.LastHarassTick > 0 && self.Tick - u.Health.LastHarassTick <= memory);
            if (harasser != null) return harasser;
            Unit regular = units.Closest(self.Position, radius, u => IsValidEnemy(self, u) && !u.IsChampion);
            return regular != null ? regular : units.Closest(self.Position, radius, u => IsValidEnemy(self, u));
        }

        public static bool ShouldDrop(Unit self, Unit target, float leash) =>
            !IsValidEnemy(self, target) || UnitRegistry.FlatDistance(self.Position, target.Position) > leash;

        public static bool IsRetargetTick(Unit self, int period) => (self.Tick + (int)(self.SortKey % (uint)Mathf.Max(1, period))) % Mathf.Max(1, period) == 0;
    }
}
