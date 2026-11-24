using System;

namespace MangaLegends.Core.Simulation
{
    public static class TickTime
    {
        public static int SecondsToTicks(float seconds, int tickRate)
        {
            if (tickRate <= 0) throw new ArgumentOutOfRangeException(nameof(tickRate));
            return seconds <= 0f ? 0 : (int)Math.Ceiling(seconds * tickRate - 1e-4f);
        }

        public static float TicksToSeconds(int ticks, int tickRate)
        {
            if (tickRate <= 0) throw new ArgumentOutOfRangeException(nameof(tickRate));
            return (float)ticks / tickRate;
        }
    }
}
