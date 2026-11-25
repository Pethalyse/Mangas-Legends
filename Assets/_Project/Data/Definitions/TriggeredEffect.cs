using System;
using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Data.Effects;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [Serializable]
    public sealed class TriggeredEffect
    {
        [SerializeField] private ItemTrigger _trigger;
        [SerializeField, Tooltip("Only when the other unit is of one of these kinds; empty = any.")] private List<UnitKind> _otherKinds = new List<UnitKind>();
        [SerializeReference, TypeSelector] private List<EffectDefinition> _effects = new List<EffectDefinition>();

        public TriggeredEffect() { }

        public TriggeredEffect(ItemTrigger trigger, List<EffectDefinition> effects)
        {
            _trigger = trigger;
            _effects = effects;
        }

        public ItemTrigger Trigger => _trigger;
        public IReadOnlyList<EffectDefinition> Effects => _effects;
        public IReadOnlyList<UnitKind> OtherKinds => _otherKinds;

        public bool Accepts(UnitKind kind) => _otherKinds == null || _otherKinds.Count == 0 || _otherKinds.Contains(kind);

        public void Validate(string ownerId, ValidationReport report) => Validate(ownerId, 1, report);

        public void Validate(string ownerId, int rankCount, ValidationReport report)
        {
            if (_effects.Count == 0) report.Warning(ownerId, $"Trigger {_trigger} has no effect.");
            foreach (EffectDefinition effect in _effects)
            {
                if (effect == null) report.Error(ownerId, "A triggered effect entry is empty.");
                else effect.Validate(ownerId, rankCount, report);
            }
        }
    }
}
