using System;
using MangaLegends.Core.Stats;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public sealed class StatEntry
    {
        [SerializeField] private StatType _stat;
        [SerializeField] private float _base;
        [SerializeField] private float _growthPerLevel;

        public StatEntry()
        {
        }

        public StatEntry(StatType stat, float baseValue, float growthPerLevel)
        {
            _stat = stat;
            _base = baseValue;
            _growthPerLevel = growthPerLevel;
        }

        public StatType Stat => _stat;
        public float Base => _base;
        public float GrowthPerLevel => _growthPerLevel;
    }
}
