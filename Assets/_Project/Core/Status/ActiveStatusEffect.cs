namespace MangaLegends.Core.Status
{
    public struct ActiveStatusEffect
    {
        public ActiveStatusEffect(int instanceId, StatusEffectSpec spec, int sourceId, int appliedAtTick)
        {
            InstanceId = instanceId;
            Spec = spec;
            SourceId = sourceId;
            AppliedAtTick = appliedAtTick;
            ExpiresAtTick = spec.IsPermanent ? 0 : appliedAtTick + spec.DurationTicks;
            Stacks = 1;
        }

        public int InstanceId { get; }
        public StatusEffectSpec Spec { get; }
        public int SourceId { get; }
        public int AppliedAtTick { get; }
        public int ExpiresAtTick { get; internal set; }
        public int Stacks { get; internal set; }

        public bool IsExpired(int currentTick) => ExpiresAtTick > 0 && ExpiresAtTick <= currentTick;
    }
}
