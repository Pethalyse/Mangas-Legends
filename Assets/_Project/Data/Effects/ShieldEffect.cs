using System;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class ShieldEffect : EffectDefinition
    {
        [SerializeField] private EffectTarget _target = EffectTarget.Target;
        [SerializeField] private ScalingValueData _amount = new ScalingValueData();
        [SerializeField] private ShieldFilter _filter = ShieldFilter.Any;
        [SerializeField, Min(0f)] private float _durationSeconds = 2f;
        [SerializeField] private bool _blocksCrowdControl;

        public EffectTarget Target => _target;
        public ScalingValueData Amount => _amount;
        public ShieldFilter Filter => _filter;
        public float DurationSeconds => _durationSeconds;
        public bool BlocksCrowdControl => _blocksCrowdControl;

        public override void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _amount.Validate(ownerId, "Shield amount", maxRank, report);
    }
}
