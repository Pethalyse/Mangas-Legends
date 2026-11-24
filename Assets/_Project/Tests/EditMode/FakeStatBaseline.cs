using System.Collections.Generic;
using MangaLegends.Core.Stats;

namespace MangaLegends.Tests.EditMode
{
    internal sealed class FakeStatBaseline : IStatBaseline
    {
        private readonly Dictionary<StatType, float> _base = new Dictionary<StatType, float>();
        private readonly Dictionary<StatType, float> _growth = new Dictionary<StatType, float>();

        public FakeStatBaseline With(StatType stat, float value, float growth = 0f)
        {
            _base[stat] = value;
            _growth[stat] = growth;
            return this;
        }

        public float GetBase(StatType stat) => _base.TryGetValue(stat, out float value) ? value : 0f;
        public float GetGrowthPerLevel(StatType stat) => _growth.TryGetValue(stat, out float value) ? value : 0f;

        public StatBlock ToBlock() => new StatBlock(this);
    }
}
