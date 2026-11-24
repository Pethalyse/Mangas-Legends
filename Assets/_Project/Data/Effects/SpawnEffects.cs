using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class LaunchProjectileEffect : EffectDefinition
    {
        [SerializeField] private ProjectileDefinition _projectile;
        [SerializeField, Min(1)] private int _count = 1;
        [SerializeField, Range(0f, 180f)] private float _spreadAngle;

        public ProjectileDefinition Projectile => _projectile;
        public int Count => _count;
        public float SpreadAngle => _spreadAngle;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_projectile == null) report.Error(ownerId, "LaunchProjectileEffect has no projectile assigned.");
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
        }
    }

    [Serializable]
    public sealed class SummonEffect : EffectDefinition
    {
        [SerializeField] private UnitDefinition _unit;
        [SerializeField, Min(1)] private int _count = 1;
        [SerializeField, Min(0f)] private float _durationSeconds;
        [SerializeField] private bool _controllable;

        public UnitDefinition Unit => _unit;
        public int Count => _count;
        public float DurationSeconds => _durationSeconds;
        public bool Controllable => _controllable;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_unit == null) report.Error(ownerId, "SummonEffect has no unit assigned.");
        }
    }

    [Serializable]
    public sealed class SwapAbilitySetEffect : EffectDefinition
    {
        [SerializeField, Min(-1)] private int _alternateSetIndex;
        [SerializeField] private bool _resetCooldowns;
        [SerializeField, Min(0f)] private float _durationSeconds;

        public int AlternateSetIndex => _alternateSetIndex;
        public bool ResetCooldowns => _resetCooldowns;
        public float DurationSeconds => _durationSeconds;
    }
}
