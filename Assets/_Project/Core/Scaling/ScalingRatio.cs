using System;
using System.Collections.Generic;
using MangaLegends.Core.Stats;

namespace MangaLegends.Core.Scaling
{
    public readonly struct ScalingRatio
    {
        private readonly float[] _ratioByRank;

        public ScalingRatio(ScalingSource source, float ratio, StatType stat = StatType.AttackDamage)
            : this(source, new[] { ratio }, stat)
        {
        }

        public ScalingRatio(ScalingSource source, IReadOnlyList<float> ratioByRank, StatType stat = StatType.AttackDamage)
        {
            if (ratioByRank == null || ratioByRank.Count == 0) throw new ArgumentException("At least one rank is required.", nameof(ratioByRank));
            Source = source;
            Stat = stat;
            _ratioByRank = new float[ratioByRank.Count];
            for (int i = 0; i < ratioByRank.Count; i++) _ratioByRank[i] = ratioByRank[i];
        }

        public ScalingSource Source { get; }
        public StatType Stat { get; }
        public float Ratio => RatioAt(1);

        public float RatioAt(int rank)
        {
            if (_ratioByRank == null || _ratioByRank.Length == 0) return 0f;
            int index = Math.Min(Math.Max(rank, 1), _ratioByRank.Length) - 1;
            return _ratioByRank[index];
        }
    }
}
