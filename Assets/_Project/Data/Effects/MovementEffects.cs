using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Serialization;
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
        Knockup,
        Random
    }

    [Serializable]
    public sealed class DashEffect : EffectDefinition
    {
        [SerializeField] private DashDestination _destination = DashDestination.TowardsAimPoint;
        [SerializeField] private RankedFloat _maxDistance = new RankedFloat(4f);
        [SerializeField] private RankedFloat _speed = new RankedFloat(20f);
        [SerializeField] private bool _passesThroughWalls;

        public DashDestination Destination => _destination;
        public float MaxDistanceAt(int rank) => _maxDistance.At(rank);
        public float SpeedAt(int rank) => _speed.At(rank);
        public bool PassesThroughWalls => _passesThroughWalls;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            _maxDistance.Validate(ownerId, "Dash distance", maxRank, report);
            _speed.Validate(ownerId, "Dash speed", maxRank, report);
            if (_destination != DashDestination.ToTarget && _maxDistance.At(1) <= 0f) report.Error(ownerId, "Dash distance must be positive.");
        }
    }

    [Serializable]
    public sealed class DisplaceEffect : EffectDefinition
    {
        [SerializeField] private DisplacementKind _kind = DisplacementKind.Knockback;
        [SerializeField] private RankedFloat _distance = new RankedFloat(2f);
        [SerializeField] private RankedFloat _durationSeconds = new RankedFloat(0.5f);
        [SerializeField, Tooltip("Push or pull from the impact point (an area centre) instead of the caster.")] private bool _fromImpactPoint;

        public DisplacementKind Kind => _kind;
        public float DistanceAt(int rank) => _distance.At(rank);
        public float DurationSecondsAt(int rank) => _durationSeconds.At(rank);
        public bool FromImpactPoint => _fromImpactPoint;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            _distance.Validate(ownerId, "Displacement distance", maxRank, report);
            _durationSeconds.Validate(ownerId, "Displacement duration", maxRank, report);
        }
    }

    [Serializable]
    public sealed class TeleportEffect : EffectDefinition
    {
        [SerializeField] private UnitKind[] _allowedDestinations = { UnitKind.Tower, UnitKind.Minion };
        [SerializeField, Min(0f)] private float _arrivalOffset = 1f;

        public UnitKind[] AllowedDestinations => _allowedDestinations;
        public float ArrivalOffset => _arrivalOffset;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_allowedDestinations == null || _allowedDestinations.Length == 0) report.Error(ownerId, "Teleport has no allowed destination.");
        }
    }

    [Serializable]
    public sealed class SwapPositionsEffect : EffectDefinition
    {
        [SerializeField] private bool _includeCaster = true;

        public bool IncludeCaster => _includeCaster;
    }
}
