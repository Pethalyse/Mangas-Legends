using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class LaunchProjectileEffect : EffectDefinition
    {
        [SerializeField] private ProjectileDefinition _projectile;
        [SerializeField] private RankedInt _count = new RankedInt(1);
        [SerializeField] private RankedFloat _spreadAngle = new RankedFloat(0f);

        public ProjectileDefinition Projectile => _projectile;
        public int CountAt(int rank) => Math.Max(1, _count.At(rank));
        public float SpreadAngleAt(int rank) => _spreadAngle.At(rank);

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            _count.Validate(ownerId, "Projectile count", maxRank, report);
            _spreadAngle.Validate(ownerId, "Projectile spread", maxRank, report);
            if (_projectile == null) report.Error(ownerId, "LaunchProjectileEffect has no projectile assigned.");
            else _projectile.ValidateForOwner(ownerId, maxRank, report);
        }
    }

    [Serializable]
    public sealed class CreateAreaEffect : EffectDefinition
    {
        [SerializeField] private AreaDefinition _area;
        [SerializeField] private bool _atAimPoint = true;

        public AreaDefinition Area => _area;
        public bool AtAimPoint => _atAimPoint;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_area == null) report.Error(ownerId, "CreateAreaEffect has no area assigned.");
            else _area.ValidateForOwner(ownerId, maxRank, report);
        }
    }

    [Serializable]
    public sealed class SummonEffect : EffectDefinition
    {
        [SerializeField] private UnitDefinition _unit;
        [SerializeField] private RankedInt _count = new RankedInt(1);
        [SerializeField] private RankedFloat _durationSeconds = new RankedFloat(0f);
        [SerializeField] private bool _controllable;

        public UnitDefinition Unit => _unit;
        public int CountAt(int rank) => Math.Max(1, _count.At(rank));
        public float DurationSecondsAt(int rank) => _durationSeconds.At(rank);
        public bool Controllable => _controllable;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            _count.Validate(ownerId, "Summon count", maxRank, report);
            _durationSeconds.Validate(ownerId, "Summon duration", maxRank, report);
            if (_unit == null) report.Error(ownerId, "SummonEffect has no unit assigned.");
        }
    }

    [Serializable]
    public sealed class SwapAbilitySetEffect : EffectDefinition
    {
        [SerializeField, Min(-1)] private int _alternateSetIndex;
        [SerializeField] private bool _resetCooldowns;
        [SerializeField] private RankedFloat _durationSeconds = new RankedFloat(0f);

        public int AlternateSetIndex => _alternateSetIndex;
        public bool ResetCooldowns => _resetCooldowns;
        public float DurationSecondsAt(int rank) => _durationSeconds.At(rank);

        public override void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _durationSeconds.Validate(ownerId, "Ability set duration", maxRank, report);
    }
}
