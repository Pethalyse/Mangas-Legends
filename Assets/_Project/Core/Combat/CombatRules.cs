namespace MangaLegends.Core.Combat
{
    public readonly struct CombatRules
    {
        public CombatRules(float resistanceScale, float minimumDamage)
        {
            ResistanceScale = resistanceScale;
            MinimumDamage = minimumDamage;
        }

        public float ResistanceScale { get; }
        public float MinimumDamage { get; }
    }
}
