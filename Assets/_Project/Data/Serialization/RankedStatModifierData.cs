using System;
using MangaLegends.Core.Content;
using MangaLegends.Core.Stats;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public sealed class RankedStatModifierData
    {
        [SerializeField] private StatType _stat;
        [SerializeField] private ModifierKind _kind;
        [SerializeField] private RankedFloat _value = new RankedFloat(0f);

        public StatType Stat => _stat;
        public ModifierKind Kind => _kind;
        public RankedFloat Value => _value;

        public StatModifier ToModifier(int rank, int sourceId, int stacks = 1) => new StatModifier(_stat, _kind, _value.At(rank) * stacks, sourceId);

        public void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _value.Validate(ownerId, $"Modifier {_stat}", maxRank, report);
    }
}
