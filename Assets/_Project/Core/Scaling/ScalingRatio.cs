using MangaLegends.Core.Stats;

namespace MangaLegends.Core.Scaling
{
    public readonly struct ScalingRatio
    {
        public ScalingRatio(ScalingSource source, float ratio, StatType stat = StatType.AttackDamage)
        {
            Source = source;
            Ratio = ratio;
            Stat = stat;
        }

        public ScalingSource Source { get; }
        public StatType Stat { get; }
        public float Ratio { get; }
    }
}
