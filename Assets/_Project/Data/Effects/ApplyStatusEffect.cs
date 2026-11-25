using System;
using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class ApplyStatusEffect : EffectDefinition
    {
        [SerializeField] private EffectTarget _target = EffectTarget.Target;
        [SerializeField] private StatusEffectDefinition _status;
        [SerializeField] private RankedInt _stacks = new RankedInt(1);
        [SerializeField] private RankedFloat _durationOverrideSeconds = new RankedFloat(0f);
        [SerializeField] private List<RankedStatModifierData> _modifierOverrides = new List<RankedStatModifierData>();
        [SerializeField, Tooltip("Only applied when the target carries this status with a stack count in the range below.")] private StatusEffectDefinition _requiredStatus;
        [SerializeField, Min(0)] private int _requiredMinStacks;
        [SerializeField, Min(0), Tooltip("0 = no upper bound.")] private int _requiredMaxStacks;

        public EffectTarget Target => _target;
        public StatusEffectDefinition Status => _status;
        public IReadOnlyList<RankedStatModifierData> ModifierOverrides => _modifierOverrides;
        public StatusEffectDefinition RequiredStatus => _requiredStatus;

        public bool AcceptsStacks(int stacks) => stacks >= _requiredMinStacks && (_requiredMaxStacks <= 0 || stacks <= _requiredMaxStacks);

        public int StacksAt(int rank) => Math.Max(1, _stacks.At(rank));

        public float DurationSecondsAt(int rank)
        {
            float overrideSeconds = _durationOverrideSeconds.At(rank);
            if (overrideSeconds > 0f) return overrideSeconds;
            return _status != null ? _status.DurationSeconds : 0f;
        }

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_status == null) report.Error(ownerId, "ApplyStatusEffect has no status assigned.");
            _stacks.Validate(ownerId, "Status stacks", maxRank, report);
            _durationOverrideSeconds.Validate(ownerId, "Status duration", maxRank, report);
            foreach (RankedStatModifierData modifier in _modifierOverrides) modifier.Validate(ownerId, maxRank, report);
        }
    }
}
