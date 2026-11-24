namespace MangaLegends.Core.Stats
{
    public enum StatType
    {
        MaxHealth,
        HealthRegen,
        MaxMana,
        ManaRegen,
        AttackDamage,
        AbilityPower,
        Armor,
        MagicResist,
        AttackSpeed,
        MoveSpeed,
        AttackRange,
        CritChance,
        CritDamage,
        Lethality,
        ArmorPenetration,
        MagicPenetrationFlat,
        MagicPenetration,
        LifeSteal,
        HealPower,
        AbilityHaste,
        VisionRange
    }

    public static class StatTypes
    {
        public static readonly int Count = System.Enum.GetValues(typeof(StatType)).Length;
    }
}
