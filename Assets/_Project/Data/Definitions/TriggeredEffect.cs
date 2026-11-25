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
        [SerializeReference, TypeSelector] private List<EffectDefinition> _effects = new List<EffectDefinition>();

        public TriggeredEffect() { }

        public TriggeredEffect(ItemTrigger trigger, List<EffectDefinition> effects)
        {
            _trigger = trigger;
            _effects = effects;
        }

        public ItemTrigger Trigger => _trigger;
        public IReadOnlyList<EffectDefinition> Effects => _effects;

        public void Validate(string ownerId, ValidationReport report)
        {
            if (_effects.Count == 0) report.Warning(ownerId, $"Trigger {_trigger} has no effect.");
            foreach (EffectDefinition effect in _effects)
            {
                if (effect == null) report.Error(ownerId, "A triggered effect entry is empty.");
                else effect.Validate(ownerId, 1, report);
            }
        }
    }
}
