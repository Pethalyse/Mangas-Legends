using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class ApplyStatusEffect : EffectDefinition
    {
        [SerializeField] private EffectTarget _target = EffectTarget.Target;
        [SerializeField] private StatusEffectDefinition _status;
        [SerializeField, Min(1)] private int _stacks = 1;
        [SerializeField, Min(0f)] private float _durationOverrideSeconds;

        public EffectTarget Target => _target;
        public StatusEffectDefinition Status => _status;
        public int Stacks => _stacks;
        public float DurationSeconds => _durationOverrideSeconds > 0f ? _durationOverrideSeconds : _status != null ? _status.DurationSeconds : 0f;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_status == null) report.Error(ownerId, "ApplyStatusEffect has no status assigned.");
        }
    }
}
