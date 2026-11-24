using System;
using System.Collections.Generic;

namespace MangaLegends.Core.Stats
{
    public sealed class StatBlock : IStatReader
    {
        private readonly IStatBaseline _baseline;
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private readonly float[] _cache = new float[StatTypes.Count];
        private readonly bool[] _dirty = new bool[StatTypes.Count];
        private int _level = 1;

        public StatBlock(IStatBaseline baseline)
        {
            _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
            MarkAllDirty();
        }

        public int Level => _level;
        public IReadOnlyList<StatModifier> Modifiers => _modifiers;

        public void SetLevel(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (level == _level) return;
            _level = level;
            MarkAllDirty();
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            _dirty[(int)modifier.Stat] = true;
        }

        public int RemoveModifiersFromSource(int sourceId)
        {
            int removed = 0;
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].SourceId != sourceId) continue;
                _dirty[(int)_modifiers[i].Stat] = true;
                _modifiers.RemoveAt(i);
                removed++;
            }
            return removed;
        }

        public void ClearModifiers()
        {
            _modifiers.Clear();
            MarkAllDirty();
        }

        public float GetBase(StatType stat) =>
            _baseline.GetBase(stat) + _baseline.GetGrowthPerLevel(stat) * (_level - 1);

        public float GetBonus(StatType stat) => Get(stat) - GetBase(stat);

        public float Get(StatType stat)
        {
            int index = (int)stat;
            if (!_dirty[index]) return _cache[index];
            _cache[index] = Compute(stat);
            _dirty[index] = false;
            return _cache[index];
        }

        private float Compute(StatType stat)
        {
            float flat = 0f;
            float percentAdd = 0f;
            float percentMult = 1f;
            foreach (StatModifier modifier in _modifiers)
            {
                if (modifier.Stat != stat) continue;
                switch (modifier.Kind)
                {
                    case ModifierKind.Flat:
                        flat += modifier.Value;
                        break;
                    case ModifierKind.PercentAdd:
                        percentAdd += modifier.Value;
                        break;
                    case ModifierKind.PercentMult:
                        percentMult *= 1f + modifier.Value;
                        break;
                }
            }
            return (GetBase(stat) + flat) * (1f + percentAdd) * percentMult;
        }

        private void MarkAllDirty()
        {
            for (int i = 0; i < _dirty.Length; i++) _dirty[i] = true;
        }
    }
}
