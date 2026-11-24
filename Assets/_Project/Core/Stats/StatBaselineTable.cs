namespace MangaLegends.Core.Stats
{
    public sealed class StatBaselineTable : IStatBaseline
    {
        private readonly float[] _base = new float[StatTypes.Count];
        private readonly float[] _growth = new float[StatTypes.Count];

        public StatBaselineTable Set(StatType stat, float baseValue, float growthPerLevel = 0f)
        {
            _base[(int)stat] = baseValue;
            _growth[(int)stat] = growthPerLevel;
            return this;
        }

        public float GetBase(StatType stat) => _base[(int)stat];
        public float GetGrowthPerLevel(StatType stat) => _growth[(int)stat];
    }
}
