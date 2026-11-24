using System;
using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    public enum DashDestination
    {
        TowardsAimPoint,
        ToTarget,
        BehindTarget,
        Blink
    }

    public enum DisplacementKind
    {
        Knockback,
        Pull,
        Knockup
    }

    [Serializable]
    public sealed class DashEffect : EffectDefinition
    {
        [SerializeField] private DashDestination _destination = DashDestination.TowardsAimPoint;
        [SerializeField, Min(0f)] private float _maxDistance = 4f;
        [SerializeField, Min(0.1f)] private float _speed = 20f;
        [SerializeField] private bool _passesThroughWalls;

        public DashDestination Destination => _destination;
        public float MaxDistance => _maxDistance;
        public float Speed => _speed;
        public bool PassesThroughWalls => _passesThroughWalls;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_destination != DashDestination.ToTarget && _maxDistance <= 0f) report.Error(ownerId, "Dash distance must be positive.");
        }
    }

    [Serializable]
    public sealed class DisplaceEffect : EffectDefinition
    {
        [SerializeField] private DisplacementKind _kind = DisplacementKind.Knockback;
        [SerializeField, Min(0f)] private float _distance = 2f;
        [SerializeField, Min(0f)] private float _durationSeconds = 0.5f;

        public DisplacementKind Kind => _kind;
        public float Distance => _distance;
        public float DurationSeconds => _durationSeconds;
    }

    [Serializable]
    public sealed class SwapPositionsEffect : EffectDefinition
    {
        [SerializeField] private bool _includeCaster = true;

        public bool IncludeCaster => _includeCaster;
    }
}
