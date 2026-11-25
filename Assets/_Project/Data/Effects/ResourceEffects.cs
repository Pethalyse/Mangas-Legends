using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    // Takes a share of the target's current mana and gives it to the caster.
    [Serializable]
    public sealed class DrainResourceEffect : EffectDefinition
    {
        [SerializeField] private RankedFloat _currentRatio = new RankedFloat(0.1f);

        public float CurrentRatioAt(int rank) => Mathf.Clamp01(_currentRatio.At(rank));

        public override void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _currentRatio.Validate(ownerId, "Drained share", maxRank, report);
    }

    // The caster pays health (never lethal): Giorno's life transfer.
    [Serializable]
    public sealed class PayHealthEffect : EffectDefinition
    {
        [SerializeField] private ScalingValueData _amount = new ScalingValueData();

        public ScalingValueData Amount => _amount;

        public override void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _amount.Validate(ownerId, "Health paid", maxRank, report);
    }

    // Lets the caster start a new basic attack at once.
    [Serializable]
    public sealed class ResetAttackEffect : EffectDefinition
    {
    }
}
