using System;
using System.Collections.Generic;

namespace MangaLegends.Core.Abilities
{
    public static class RankValues
    {
        public static float At(IReadOnlyList<float> values, int rank)
        {
            if (values == null || values.Count == 0) return 0f;
            int index = Math.Min(Math.Max(rank, 1), values.Count) - 1;
            return values[index];
        }
    }
}
