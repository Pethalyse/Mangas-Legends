using System;
using MangaLegends.Core.Abilities;
using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    // Cuts the remaining cooldown of one of the caster's abilities by a fraction.
    [Serializable]
    public sealed class ReduceCooldownEffect : EffectDefinition
    {
        [SerializeField] private AbilitySlot _slot = AbilitySlot.A;
        [SerializeField] private RankedFloat _remainingFraction = new RankedFloat(0.5f);

        public AbilitySlot Slot => _slot;
        public float RemainingFractionAt(int rank) => Mathf.Clamp01(_remainingFraction.At(rank));

        public override void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _remainingFraction.Validate(ownerId, "Cooldown reduction", maxRank, report);
    }
}
