namespace MangaLegends.Core.Combat
{
    public struct Shield
    {
        public Shield(int id, float amount, ShieldFilter filter, int expiresAtTick, bool blocksCrowdControl = false)
        {
            Id = id;
            Remaining = amount;
            Filter = filter;
            ExpiresAtTick = expiresAtTick;
            BlocksCrowdControl = blocksCrowdControl;
        }

        public int Id { get; }
        public float Remaining { get; internal set; }
        public ShieldFilter Filter { get; }
        public int ExpiresAtTick { get; }
        public bool BlocksCrowdControl { get; }

        public bool Accepts(DamageType type) =>
            Filter == ShieldFilter.Any
            || (Filter == ShieldFilter.Physical && type == DamageType.Physical)
            || (Filter == ShieldFilter.Magical && type == DamageType.Magical);
    }
}
