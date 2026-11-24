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
        [SerializeField] private RankedFloat _durationSeconds = new RankedFloat(2f);
        [SerializeField] private bool _blocksCrowdControl;

        public EffectTarget Target => _target;
        public ScalingValueData Amount => _amount;
        public ShieldFilter Filter => _filter;
        public bool BlocksCrowdControl => _blocksCrowdControl;

        public float DurationSecondsAt(int rank) => _durationSeconds.At(rank);

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            _amount.Validate(ownerId, "Shield amount", maxRank, report);
            _durationSeconds.Validate(ownerId, "Shield duration", maxRank, report);
        }
    }
}
