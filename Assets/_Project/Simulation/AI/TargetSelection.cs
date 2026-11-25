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

        // LoL minion priorities (lower wins): 1 enemy champion hurting an allied champion, 2 enemy minion attacking an allied
        // champion, 3 enemy minion attacking an allied minion, 4 tower attacking an allied minion, 5 closest enemy minion,
        // 6 closest enemy champion, 7 structures. Hitting a minion no longer draws aggro on a champion (patch 26.10).
        public static int MinionPriority(Unit self, Unit enemy)
        {
            Unit target = enemy.Attack != null && self.TryResolve(enemy.Attack.Target, out Unit t) ? t : null;
            bool targetIsAlly = target != null && target.Team == self.Team;
            if (enemy.IsChampion) return HurtsAlliedChampion(self, enemy) ? 1 : 6;
            if (enemy.Kind == UnitKind.Minion || enemy.Kind == UnitKind.Summon)
            {
                if (targetIsAlly && target.IsChampion) return 2;
                return targetIsAlly ? 3 : 5;
            }
            if (enemy.Kind == UnitKind.Tower && targetIsAlly && target.Kind == UnitKind.Minion) return 4;
            return 7;
        }

        public static Unit PickForMinion(Unit self, float radius, Unit current)
        {
            Unit best = null;
            int bestPriority = int.MaxValue;
            float bestDistance = float.MaxValue;
            foreach (Unit unit in self.Services.Units.All)
            {
                if (unit == null || !unit.IsAlive) continue;
                float distance = UnitRegistry.FlatDistance(self.Position, unit.Position) - unit.Radius;
                if (distance > radius || !IsValidEnemy(self, unit)) continue;
                int priority = MinionPriority(self, unit);
                if (priority < bestPriority || priority == bestPriority && (distance < bestDistance || Mathf.Approximately(distance, bestDistance) && unit.SortKey < best.SortKey))
                {
                    best = unit;
                    bestPriority = priority;
                    bestDistance = distance;
                }
            }
            if (current != null && !ShouldDrop(self, current, radius) && (best == null || MinionPriority(self, current) <= bestPriority)) return current;
            return best;
        }

        private static bool HurtsAlliedChampion(Unit self, Unit enemy)
        {
            int memory = TickTime.SecondsToTicks(HarassMemorySeconds, self.TickRate);
            foreach (Unit ally in self.Services.Units.All)
            {
                if (ally == null || !ally.IsChampion || ally.Team != self.Team || !ally.IsAlive) continue;
                if (ally.Health.LastAttacker == enemy.Object.Id && self.Tick - ally.Health.LastDamagedTick <= memory) return true;
            }
            return false;
        }

        public static bool ShouldDrop(Unit self, Unit target, float leash) =>
            !IsValidEnemy(self, target) || UnitRegistry.FlatDistance(self.Position, target.Position) > leash;

        public static bool IsRetargetTick(Unit self, int period) => (self.Tick + (int)(self.SortKey % (uint)Mathf.Max(1, period))) % Mathf.Max(1, period) == 0;
    }
}
