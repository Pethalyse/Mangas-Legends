namespace MangaLegends.Core.Combat
{
    public readonly struct DamageRequest
    {
        public DamageRequest(float amount, DamageType type, DamageFlags flags = DamageFlags.None)
        {
            Amount = amount;
            Type = type;
            Flags = flags;
        }

        public float Amount { get; }
        public DamageType Type { get; }
        public DamageFlags Flags { get; }

        public bool Has(DamageFlags flag) => (Flags & flag) == flag;
    }
}
