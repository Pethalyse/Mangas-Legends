namespace MangaLegends.Core.Stats
{
    public interface IStatReader
    {
        float Get(StatType stat);
        float GetBase(StatType stat);
        float GetBonus(StatType stat);
    }
}
