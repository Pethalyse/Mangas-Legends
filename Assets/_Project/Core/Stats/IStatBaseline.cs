namespace MangaLegends.Core.Stats
{
    public interface IStatBaseline
    {
        float GetBase(StatType stat);
        float GetGrowthPerLevel(StatType stat);
    }
}
