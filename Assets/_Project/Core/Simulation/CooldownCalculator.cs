using System;

namespace MangaLegends.Core.Simulation
{
    public static class CooldownCalculator
    {
        public static float ApplyHaste(float baseCooldownSeconds, float abilityHaste, float hasteScale)
        {
            if (baseCooldownSeconds <= 0f) return 0f;
            return baseCooldownSeconds * hasteScale / (hasteScale + Math.Max(0f, abilityHaste));
        }

        public static int ToTicks(float baseCooldownSeconds, float abilityHaste, float hasteScale, int tickRate) =>
            TickTime.SecondsToTicks(ApplyHaste(baseCooldownSeconds, abilityHaste, hasteScale), tickRate);
    }
}
