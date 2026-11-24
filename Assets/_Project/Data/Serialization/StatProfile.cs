using System;
using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Core.Stats;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public sealed class StatProfile
    {
        [SerializeField] private List<StatEntry> _entries = new List<StatEntry>();

        public IReadOnlyList<StatEntry> Entries => _entries;

        public StatBaselineTable ToBaseline()
        {
            StatBaselineTable table = new StatBaselineTable();
            foreach (StatEntry entry in _entries) table.Set(entry.Stat, entry.Base, entry.GrowthPerLevel);
            return table;
        }

        public void Validate(string ownerId, ValidationReport report)
        {
            HashSet<StatType> seen = new HashSet<StatType>();
            foreach (StatEntry entry in _entries)
                if (!seen.Add(entry.Stat)) report.Error(ownerId, $"Stat {entry.Stat} is defined twice.");
            if (!seen.Contains(StatType.MaxHealth)) report.Warning(ownerId, "MaxHealth is not defined.");
        }
    }
}
