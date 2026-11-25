using System;
using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Core.Scaling;
using MangaLegends.Core.Stats;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public sealed class ScalingValueData
    {
        [SerializeField] private float[] _baseByRank = { 0f };
        [SerializeField] private List<ScalingRatioData> _ratios = new List<ScalingRatioData>();

        [NonSerialized] private ScalingValue _cached;

        public IReadOnlyList<float> BaseByRank => _baseByRank;

        public IEnumerable<ScalingRatio> Ratios
        {
            get
            {
                foreach (ScalingRatioData ratio in _ratios) yield return ratio.ToRatio();
            }
        }

        public float Evaluate(int rank, in ScalingContext context) => ToScalingValue().Evaluate(rank, context);

        public ScalingValue ToScalingValue()
        {
            if (_cached != null) return _cached;
            List<ScalingRatio> ratios = new List<ScalingRatio>(_ratios.Count);
            foreach (ScalingRatioData ratio in _ratios) ratios.Add(ratio.ToRatio());
            _cached = new ScalingValue(_baseByRank != null && _baseByRank.Length > 0 ? _baseByRank : new[] { 0f }, ratios);
            return _cached;
        }

        public void Validate(string ownerId, string field, int maxRank, ValidationReport report)
        {
            report.CheckRankArray(ownerId, field, _baseByRank?.Length ?? 0, maxRank);
            foreach (ScalingRatioData ratio in _ratios)
                report.CheckRankArray(ownerId, field + " ratio", ratio.RankCount, maxRank);
        }

        [Serializable]
        public sealed class ScalingRatioData
        {
            [SerializeField] private ScalingSource _source;
            [SerializeField] private StatType _stat;
            [SerializeField] private float[] _ratioByRank = { 0f };

            public int RankCount => _ratioByRank?.Length ?? 0;

            public ScalingRatio ToRatio() => new ScalingRatio(_source, RankCount > 0 ? _ratioByRank : new[] { 0f }, _stat);
        }
    }
}
