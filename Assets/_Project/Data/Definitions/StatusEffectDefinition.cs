using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Core.Simulation;
using MangaLegends.Core.Status;
using MangaLegends.Data.Effects;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Status Effect", fileName = "STATUS_New")]
    public sealed class StatusEffectDefinition : ContentDefinition
    {
        [SerializeField, Min(0f)] private float _durationSeconds = 1f;
        [SerializeField] private StatusRestriction _restrictions;
        [SerializeField] private StackingRule _stacking = StackingRule.Refresh;
        [SerializeField, Min(1)] private int _maxStacks = 1;
        [SerializeField] private bool _isCrowdControl;
        [SerializeField] private List<StatModifierData> _modifiersPerStack = new List<StatModifierData>();
        [SerializeField, Min(0f)] private float _tickIntervalSeconds;
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onTick = new List<EffectDefinition>();
        [SerializeField] private List<TriggeredEffect> _triggers = new List<TriggeredEffect>();
        [SerializeField, Min(1), Tooltip("Ranks used by ranked values in tick effects: a status takes the rank of the ability that applies it.")] private int _rankCount = 1;

        public float DurationSeconds => _durationSeconds;
        public StatusRestriction Restrictions => _restrictions;
        public StackingRule Stacking => _stacking;
        public int MaxStacks => _maxStacks;
        public bool IsCrowdControl => _isCrowdControl;
        public IReadOnlyList<StatModifierData> ModifiersPerStack => _modifiersPerStack;
        public float TickIntervalSeconds => _tickIntervalSeconds;
        public IReadOnlyList<EffectDefinition> OnTick => _onTick;
        public IReadOnlyList<TriggeredEffect> Triggers => _triggers;

        public StatusEffectSpec ToSpec(int runtimeId, int tickRate) => new StatusEffectSpec(
            runtimeId,
            TickTime.SecondsToTicks(_durationSeconds, tickRate),
            _restrictions,
            _stacking,
            _maxStacks,
            _isCrowdControl);

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (_onTick.Count > 0 && _tickIntervalSeconds <= 0f) report.Error(Id, "Tick effects need a tick interval.");
            foreach (TriggeredEffect trigger in _triggers) trigger?.Validate(Id, report);
            foreach (EffectDefinition effect in _onTick)
            {
                if (effect == null) report.Error(Id, "An effect entry is empty.");
                else effect.Validate(Id, _rankCount, report);
            }
        }
    }
}
