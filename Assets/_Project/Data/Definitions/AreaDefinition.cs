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
        [SerializeField, Min(0f)] private float _radius = 3f;
        [SerializeField, Range(0f, 360f)] private float _angle = 90f;
        [SerializeField, Min(0f)] private float _width = 2f;
        [SerializeField, Min(0f)] private float _length = 5f;
        [SerializeField, Min(0f)] private float _delaySeconds;
        [SerializeField, Min(0f)] private float _durationSeconds;
        [SerializeField, Min(0f)] private float _intervalSeconds;
        [SerializeField] private bool _followsCaster;
        [SerializeField] private TeamFilter _affects = TeamFilter.Enemies;
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onActivate = new List<EffectDefinition>();
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onInterval = new List<EffectDefinition>();
        [SerializeField] private GameObject _visualPrefab;

        public AreaShape Shape => _shape;
        public float Radius => _radius;
        public float Angle => _angle;
        public float Width => _width;
        public float Length => _length;
        public float DelaySeconds => _delaySeconds;
        public float DurationSeconds => _durationSeconds;
        public float IntervalSeconds => _intervalSeconds;
        public bool FollowsCaster => _followsCaster;
        public TeamFilter Affects => _affects;
        public IReadOnlyList<EffectDefinition> OnActivate => _onActivate;
        public IReadOnlyList<EffectDefinition> OnInterval => _onInterval;
        public GameObject VisualPrefab => _visualPrefab;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (_onInterval.Count > 0 && _intervalSeconds <= 0f) report.Error(Id, "Interval effects need an interval.");
            foreach (EffectDefinition effect in _onActivate) ValidateEffect(effect, report);
            foreach (EffectDefinition effect in _onInterval) ValidateEffect(effect, report);
        }

        private void ValidateEffect(EffectDefinition effect, ValidationReport report)
        {
            if (effect == null) report.Error(Id, "An effect entry is empty.");
            else effect.Validate(Id, 1, report);
        }
    }
}
