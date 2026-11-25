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
        [SerializeField, Tooltip("Kept through death, like the drake buffs and souls.")] private bool _persistsThroughDeath;
        [SerializeField, Min(0f), Tooltip("Shared cooldown of all its triggers.")] private float _triggerCooldownSeconds;
        [SerializeField, Min(0f), Tooltip("While active, reveals and disables enemy stealthed units within this radius.")] private float _revealRadius;
        [SerializeReference, TypeSelector, Tooltip("Run when the status reaches its max stacks; the status is then removed.")] private List<EffectDefinition> _onMaxStacks = new List<EffectDefinition>();
        [SerializeField, Min(1), Tooltip("Ranks used by ranked values in tick effects: a status takes the rank of the ability that applies it.")] private int _rankCount = 1;

        [Header("Special")]
        [SerializeField, Tooltip("The unit takes no damage.")] private bool _invulnerable;
        [SerializeField, Range(0f, 1f), Tooltip("Share of ability damage (not basic attacks) ignored.")] private float _abilityDamageReduction;
        [SerializeField, Range(0f, 360f), Tooltip("0 = from every side; otherwise only for damage whose source stands within this arc around the direction the status was cast towards (Mark Evans' wall).")] private float _reductionArcDegrees;
        [SerializeField, Range(0f, 1f), Tooltip("Share of the damage taken by the unit that its source takes instead (link).")] private float _redirectToSource;
        [SerializeField, Tooltip("The unit can only attack the source of the status.")] private bool _taunt;
        [SerializeField, Tooltip("The next basic attack taken is ignored and removes the status.")] private bool _blocksNextBasicAttack;
        [SerializeField, Tooltip("Removed once one of its triggers has run (empowered next attack).")] private bool _removedOnTrigger;
        [SerializeReference, TypeSelector, Tooltip("Run when the unit is out of combat; the status is then removed.")] private List<EffectDefinition> _onOutOfCombat = new List<EffectDefinition>();

        public float DurationSeconds => _durationSeconds;
        public StatusRestriction Restrictions => _restrictions;
        public StackingRule Stacking => _stacking;
        public int MaxStacks => _maxStacks;
        public bool IsCrowdControl => _isCrowdControl;
        public IReadOnlyList<StatModifierData> ModifiersPerStack => _modifiersPerStack;
        public float TickIntervalSeconds => _tickIntervalSeconds;
        public IReadOnlyList<EffectDefinition> OnTick => _onTick;
        public IReadOnlyList<TriggeredEffect> Triggers => _triggers;
        public bool PersistsThroughDeath => _persistsThroughDeath;
        public float TriggerCooldownSeconds => _triggerCooldownSeconds;
        public float RevealRadius => _revealRadius;
        public IReadOnlyList<EffectDefinition> OnMaxStacks => _onMaxStacks;
        public bool Invulnerable => _invulnerable;
        public float AbilityDamageReduction => _abilityDamageReduction;
        public float RedirectToSource => _redirectToSource;
        public bool Taunt => _taunt;
        public bool BlocksNextBasicAttack => _blocksNextBasicAttack;
        public bool RemovedOnTrigger => _removedOnTrigger;
        public float ReductionArcDegrees => _reductionArcDegrees;
        public IReadOnlyList<EffectDefinition> OnOutOfCombat => _onOutOfCombat;

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
            foreach (TriggeredEffect trigger in _triggers) trigger?.Validate(Id, _rankCount, report);
            foreach (EffectDefinition effect in _onTick)
            {
                if (effect == null) report.Error(Id, "An effect entry is empty.");
                else effect.Validate(Id, _rankCount, report);
            }
        }
    }
}
