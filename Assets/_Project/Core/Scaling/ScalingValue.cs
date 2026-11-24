using System;
using System.Collections.Generic;

namespace MangaLegends.Core.Scaling
{
    public sealed class ScalingValue
    {
        private readonly float[] _baseByRank;
        private readonly ScalingRatio[] _ratios;

        public ScalingValue(IReadOnlyList<float> baseByRank, IReadOnlyList<ScalingRatio> ratios = null)
        {
            if (baseByRank == null || baseByRank.Count == 0) throw new ArgumentException("At least one rank is required.", nameof(baseByRank));
            _baseByRank = new float[baseByRank.Count];
            for (int i = 0; i < baseByRank.Count; i++) _baseByRank[i] = baseByRank[i];
            _ratios = new ScalingRatio[ratios?.Count ?? 0];
            for (int i = 0; i < _ratios.Length; i++) _ratios[i] = ratios[i];
        }

        public int MaxRank => _baseByRank.Length;

        public float Evaluate(int rank, in ScalingContext context)
        {
            int index = Math.Min(Math.Max(rank, 1), _baseByRank.Length) - 1;
            float value = _baseByRank[index];
            foreach (ScalingRatio ratio in _ratios) value += ratio.RatioAt(rank) * SourceValue(ratio, context);
            return value;
        }

        private static float SourceValue(in ScalingRatio ratio, in ScalingContext context)
        {
            switch (ratio.Source)
            {
                case ScalingSource.CasterTotalStat: return context.Caster?.Get(ratio.Stat) ?? 0f;
                case ScalingSource.CasterBonusStat: return context.Caster?.GetBonus(ratio.Stat) ?? 0f;
                case ScalingSource.CasterBaseStat: return context.Caster?.GetBase(ratio.Stat) ?? 0f;
                case ScalingSource.TargetMaxHealth: return context.TargetMaxHealth;
                case ScalingSource.TargetMissingHealth: return Math.Max(0f, context.TargetMaxHealth - context.TargetCurrentHealth);
                case ScalingSource.TargetCurrentHealth: return context.TargetCurrentHealth;
                case ScalingSource.Stacks: return context.Stacks;
                default: return 0f;
            }
        }
    }
}
