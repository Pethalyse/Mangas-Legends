using MangaLegends.Core.Stats;

namespace MangaLegends.Core.Scaling
{
    public readonly struct ScalingContext
    {
        public ScalingContext(IStatReader caster, float targetMaxHealth = 0f, float targetCurrentHealth = 0f, int stacks = 0)
        {
            Caster = caster;
            TargetMaxHealth = targetMaxHealth;
            TargetCurrentHealth = targetCurrentHealth;
            Stacks = stacks;
        }

        public IStatReader Caster { get; }
        public float TargetMaxHealth { get; }
        public float TargetCurrentHealth { get; }
        public int Stacks { get; }
    }
}
