namespace MangaLegends.Core.Combat
{
    public readonly struct DamageResult
    {
        public static readonly DamageResult None = new DamageResult(0f, 0f, 0f, DamageType.True, DamageFlags.None);

        public DamageResult(float raw, float afterMitigation, float absorbedByShields, DamageType type, DamageFlags flags)
        {
            Raw = raw;
            AfterMitigation = afterMitigation;
            AbsorbedByShields = absorbedByShields;
            Type = type;
            Flags = flags;
        }

        public float Raw { get; }
        public float AfterMitigation { get; }
        public float AbsorbedByShields { get; }
        public DamageType Type { get; }
        public DamageFlags Flags { get; }
        public float HealthDamage => AfterMitigation - AbsorbedByShields;
    }
}
