namespace MangaLegends.Core.Status
{
    public readonly struct StatusEffectSpec
    {
        public StatusEffectSpec(
            int id,
            int durationTicks,
            StatusRestriction restrictions = StatusRestriction.None,
            StackingRule stacking = StackingRule.Refresh,
            int maxStacks = 1,
            bool isCrowdControl = false)
        {
            Id = id;
            DurationTicks = durationTicks;
            Restrictions = restrictions;
            Stacking = stacking;
            MaxStacks = maxStacks < 1 ? 1 : maxStacks;
            IsCrowdControl = isCrowdControl;
        }

        public int Id { get; }
        public int DurationTicks { get; }
        public StatusRestriction Restrictions { get; }
        public StackingRule Stacking { get; }
        public int MaxStacks { get; }
        public bool IsCrowdControl { get; }
        public bool IsPermanent => DurationTicks <= 0;
    }
}
