using System;
using MangaLegends.Core.Stats;

namespace MangaLegends.Core.Combat
{
    public static class DamageCalculator
    {
        public static DamageResult Resolve(
            in DamageRequest request,
            IStatReader attacker,
            IStatReader defender,
            ShieldPool shields,
            in CombatRules rules)
        {
            if (request.Amount <= 0f) return DamageResult.None;

            float mitigated = Mitigate(request.Amount, request.Type, attacker, defender, rules);
            float absorbed = shields != null && !request.Has(DamageFlags.IgnoreShields)
                ? shields.Absorb(mitigated, request.Type)
                : 0f;

            return new DamageResult(request.Amount, mitigated, absorbed, request.Type, request.Flags);
        }

        public static float Mitigate(float amount, DamageType type, IStatReader attacker, IStatReader defender, in CombatRules rules)
        {
            if (type == DamageType.True || defender == null) return Math.Max(rules.MinimumDamage, amount);

            float resistance = EffectiveResistance(type, attacker, defender);
            return Math.Max(rules.MinimumDamage, amount * ResistanceMultiplier(resistance, rules.ResistanceScale));
        }

        public static float EffectiveResistance(DamageType type, IStatReader attacker, IStatReader defender)
        {
            bool physical = type == DamageType.Physical;
            float resistance = defender.Get(physical ? StatType.Armor : StatType.MagicResist);
            if (attacker == null || resistance <= 0f) return resistance;

            float percentPenetration = attacker.Get(physical ? StatType.ArmorPenetration : StatType.MagicPenetration);
            float flatPenetration = attacker.Get(physical ? StatType.Lethality : StatType.MagicPenetrationFlat);

            resistance *= 1f - Clamp01(percentPenetration);
            return Math.Max(0f, resistance - flatPenetration);
        }

        public static float ResistanceMultiplier(float resistance, float scale)
        {
            return resistance >= 0f
                ? scale / (scale + resistance)
                : 2f - scale / (scale - resistance);
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
