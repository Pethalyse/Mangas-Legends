using System;
using MangaLegends.Core.Stats;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public sealed class StatModifierData
    {
        [SerializeField] private StatType _stat;
        [SerializeField] private ModifierKind _kind;
        [SerializeField] private float _value;

        public StatModifierData()
        {
        }

        public StatModifierData(StatType stat, ModifierKind kind, float value)
        {
            _stat = stat;
            _kind = kind;
            _value = value;
        }

        public StatType Stat => _stat;
        public ModifierKind Kind => _kind;
        public float Value => _value;

        public StatModifier ToModifier(int sourceId, int stacks = 1) => new StatModifier(_stat, _kind, _value * stacks, sourceId);
    }
}
