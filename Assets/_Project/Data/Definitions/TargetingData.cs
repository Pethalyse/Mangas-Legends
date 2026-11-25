using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
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
        LowestHealthAllyInRange,
        NearestWithRequiredStatus
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
        [SerializeField] private RankedFloat _range = new RankedFloat(0f);
        [SerializeField] private RankedFloat _indicatorWidth = new RankedFloat(0f);
        [SerializeField] private RankedFloat _indicatorRadius = new RankedFloat(0f);
        [SerializeField, Tooltip("Point targeting: beyond the range, the caster walks until in range, then casts (cancelled by another order).")] private bool _walkIntoRange;
        [SerializeField, Tooltip("Unit targeting: champions only.")] private bool _championsOnly;
        [SerializeField, Tooltip("Unit targeting: the target must carry this status with at least Required Target Stacks.")] private StatusEffectDefinition _requiredTargetStatus;
        [SerializeField, Min(1)] private int _requiredTargetStacks = 1;

        public TargetingMode Mode => _mode;
        public TeamFilter ValidTargets => _validTargets;
        public TargetFallback Fallback => _fallback;
        public bool WalkIntoRange => _walkIntoRange;
        public bool ChampionsOnly => _championsOnly;
        public StatusEffectDefinition RequiredTargetStatus => _requiredTargetStatus;
        public int RequiredTargetStacks => _requiredTargetStacks;
        public float RangeAt(int rank) => _range.At(rank);
        public float IndicatorWidthAt(int rank) => _indicatorWidth.At(rank);
        public float IndicatorRadiusAt(int rank) => _indicatorRadius.At(rank);

        public void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            _range.Validate(ownerId, "Range", maxRank, report);
            _indicatorWidth.Validate(ownerId, "Indicator width", maxRank, report);
            _indicatorRadius.Validate(ownerId, "Indicator radius", maxRank, report);
        }
    }
}
