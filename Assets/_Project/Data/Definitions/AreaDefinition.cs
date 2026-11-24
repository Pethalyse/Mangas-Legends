using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Data.Effects;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    public enum AreaShape
    {
        Circle,
        Cone,
        Rectangle
    }

    [CreateAssetMenu(menuName = "MangaLegends/Area", fileName = "AREA_New")]
    public sealed class AreaDefinition : ContentDefinition
    {
        [SerializeField] private AreaShape _shape = AreaShape.Circle;
        [SerializeField] private RankedFloat _radius = new RankedFloat(3f);
        [SerializeField] private RankedFloat _angle = new RankedFloat(90f);
        [SerializeField] private RankedFloat _width = new RankedFloat(2f);
        [SerializeField] private RankedFloat _length = new RankedFloat(5f);
        [SerializeField] private RankedFloat _delaySeconds = new RankedFloat(0f);
        [SerializeField] private RankedFloat _durationSeconds = new RankedFloat(0f);
        [SerializeField] private RankedFloat _intervalSeconds = new RankedFloat(0f);
        [SerializeField] private bool _followsCaster;
        [SerializeField] private TeamFilter _affects = TeamFilter.Enemies;
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onActivate = new List<EffectDefinition>();
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onInterval = new List<EffectDefinition>();
        [SerializeField] private GameObject _visualPrefab;

        public AreaShape Shape => _shape;
        public float RadiusAt(int rank) => _radius.At(rank);
        public float AngleAt(int rank) => _angle.At(rank);
        public float WidthAt(int rank) => _width.At(rank);
        public float LengthAt(int rank) => _length.At(rank);
        public float DelaySecondsAt(int rank) => _delaySeconds.At(rank);
        public float DurationSecondsAt(int rank) => _durationSeconds.At(rank);
        public float IntervalSecondsAt(int rank) => _intervalSeconds.At(rank);
        public bool FollowsCaster => _followsCaster;
        public TeamFilter Affects => _affects;
        public IReadOnlyList<EffectDefinition> OnActivate => _onActivate;
        public IReadOnlyList<EffectDefinition> OnInterval => _onInterval;
        public GameObject VisualPrefab => _visualPrefab;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (_onInterval.Count > 0 && _intervalSeconds.At(1) <= 0f) report.Error(Id, "Interval effects need an interval.");
            foreach (EffectDefinition effect in _onActivate)
                if (effect == null) report.Error(Id, "An effect entry is empty.");
            foreach (EffectDefinition effect in _onInterval)
                if (effect == null) report.Error(Id, "An effect entry is empty.");
        }

        public void ValidateForOwner(string ownerId, int maxRank, ValidationReport report)
        {
            string field = $"Area {Id}";
            _radius.Validate(ownerId, field + " radius", maxRank, report);
            _angle.Validate(ownerId, field + " angle", maxRank, report);
            _width.Validate(ownerId, field + " width", maxRank, report);
            _length.Validate(ownerId, field + " length", maxRank, report);
            _delaySeconds.Validate(ownerId, field + " delay", maxRank, report);
            _durationSeconds.Validate(ownerId, field + " duration", maxRank, report);
            _intervalSeconds.Validate(ownerId, field + " interval", maxRank, report);
            foreach (EffectDefinition effect in _onActivate) effect?.Validate(ownerId, maxRank, report);
            foreach (EffectDefinition effect in _onInterval) effect?.Validate(ownerId, maxRank, report);
        }
    }
}
