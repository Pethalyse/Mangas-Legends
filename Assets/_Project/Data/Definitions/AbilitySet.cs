using System;
using MangaLegends.Core.Abilities;
using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [Serializable]
    public sealed class AbilitySet
    {
        [SerializeField] private string _name;
        [SerializeField] private AbilityDefinition _passive;
        [SerializeField] private AbilityDefinition _a;
        [SerializeField] private AbilityDefinition _z;
        [SerializeField] private AbilityDefinition _e;
        [SerializeField] private AbilityDefinition _r;

        public string Name => _name;

        public AbilityDefinition Get(AbilitySlot slot)
        {
            switch (slot)
            {
                case AbilitySlot.Passive: return _passive;
                case AbilitySlot.A: return _a;
                case AbilitySlot.Z: return _z;
                case AbilitySlot.E: return _e;
                case AbilitySlot.R: return _r;
                default: return null;
            }
        }

        public void Validate(string ownerId, ValidationReport report, bool allowEmptySlots)
        {
            if (allowEmptySlots) return;
            foreach (AbilitySlot slot in (AbilitySlot[])Enum.GetValues(typeof(AbilitySlot)))
                if (Get(slot) == null) report.Warning(ownerId, $"Ability set '{_name}' has no ability in slot {slot}.");
        }
    }
}
