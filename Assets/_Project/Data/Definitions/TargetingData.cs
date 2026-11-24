using System;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    public enum TargetingMode
    {
        None,
        Self,
        Unit,
        Point,
        Direction
    }

    [Flags]
    public enum TeamFilter
    {
        None = 0,
        Self = 1 << 0,
        Allies = 1 << 1,
        Enemies = 1 << 2,
        Neutral = 1 << 3
    }

    public enum TargetFallback
    {
        None,
        LowestHealthAllyInRange
    }

    public enum CastMode
    {
        Instant,
        CastTime,
        Channel,
        Toggle,
        Recast,
        Passive
    }

    [Serializable]
    public sealed class TargetingData
    {
        [SerializeField] private TargetingMode _mode = TargetingMode.Direction;
        [SerializeField] private TeamFilter _validTargets = TeamFilter.Enemies;
        [SerializeField] private TargetFallback _fallback = TargetFallback.None;
        [SerializeField, Min(0f)] private float _range;
        [SerializeField, Min(0f)] private float _indicatorWidth;
        [SerializeField, Min(0f)] private float _indicatorRadius;

        public TargetingMode Mode => _mode;
        public TeamFilter ValidTargets => _validTargets;
        public TargetFallback Fallback => _fallback;
        public float Range => _range;
        public float IndicatorWidth => _indicatorWidth;
        public float IndicatorRadius => _indicatorRadius;
    }
}
