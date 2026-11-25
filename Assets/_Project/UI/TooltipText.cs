using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Scaling;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Effects;
using MangaLegends.Data.Serialization;
using MangaLegends.Simulation.Units;

namespace MangaLegends.UI
{
    // Tooltip text built from the data, like the LoL client: name, cost, cooldown and description;
    // with Shift held, each effect with its base value, its ratios and the value of every rank.
    public static class TooltipText
    {
        private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

        public readonly struct Content
        {
            public Content(string title, string header, string body, float range)
            {
                Title = title;
                Header = header;
                Body = body;
                Range = range;
            }

            public string Title { get; }
            public string Header { get; }
            public string Body { get; }
            public float Range { get; }
        }

        public static Content Ability(AbilityDefinition ability, int rank, Unit caster, string key, bool detailed)
        {
            int shownRank = rank < 1 ? 1 : rank;
            bool passive = ability.CastMode == CastMode.Passive;
            string title = ability.DisplayName + (passive ? "" : rank > 0 ? $" (rang {rank})" : " (non appris)");
            var header = new StringBuilder();
            if (!string.IsNullOrEmpty(key)) header.Append('[').Append(key).Append("]  ");
            float cost = ability.CostAtRank(shownRank);
            bool usesMana = caster != null && caster.Resource != null && caster.Resource.UsesResource;
            if (cost > 0f && usesMana) header.Append(Number(cost)).Append(" mana  ");
            float perSecond = ability.CostPerSecondAtRank(shownRank);
            if (perSecond > 0f && usesMana) header.Append(Number(perSecond)).Append(" mana/s  ");
            float cooldown = ability.CooldownAtRank(shownRank);
            if (!passive && cooldown > 0f) header.Append("Recharge ").Append(Ranked(r => ability.CooldownAtRank(r), ability.MaxRank, shownRank, detailed)).Append(" s");
            if (ability.ChargesAt(shownRank) > 1) header.Append("  ").Append(ability.ChargesAt(shownRank)).Append(" charges");

            var body = new StringBuilder(ability.Description);
            float range = ability.Targeting.RangeAt(shownRank);
            if (detailed)
            {
                body.Append("\n");
                if (range > 0f) body.Append("\n<color=#c8aa6e>Portée</color> ").Append(Number(range)).Append(" m");
                var seen = new HashSet<object>();
                foreach (EffectDefinition effect in ability.OnCast) Describe(effect, shownRank, ability.MaxRank, caster, body, seen, 0);
            }
            return new Content(title, header.ToString(), body.ToString(), range);
        }

        public static Content Item(ItemDefinition item, Unit owner, string key, bool detailed)
        {
            var header = new StringBuilder();
            if (!string.IsNullOrEmpty(key)) header.Append('[').Append(key).Append("]  ");
            header.Append(item.IsChampionExclusive ? "Objet réservé" : $"{item.TotalCost} or");
            var body = new StringBuilder();
            foreach (StatModifierData stat in item.Stats) body.Append("+").Append(StatValue(stat)).Append(' ').Append(StatName(stat.Stat)).Append('\n');
            if (!string.IsNullOrEmpty(item.Description)) body.Append(item.Description).Append('\n');
            foreach (ItemPassive passive in item.Passives)
                body.Append("\n<color=#c8aa6e>").Append(passive.Name).Append("</color> : ").Append(passive.Description);
            float range = 0f;
            if (item.Active.Enabled)
            {
                range = item.Active.Targeting.RangeAt(1);
                body.Append("\n<color=#f0c060>Actif : ").Append(item.Active.Name).Append("</color> ").Append(item.Active.Description);
                if (item.Active.CooldownSeconds > 0f) body.Append(" (recharge ").Append(Number(item.Active.CooldownSeconds)).Append(" s)");
                if (detailed && range > 0f) body.Append("\n<color=#c8aa6e>Portée</color> ").Append(Number(range)).Append(" m");
                if (detailed)
                {
                    var seen = new HashSet<object>();
                    foreach (EffectDefinition effect in item.Active.Effects) Describe(effect, 1, 1, owner, body, seen, 0);
                }
            }
            return new Content(item.DisplayName, header.ToString(), body.ToString().TrimEnd(), range);
        }

        private static void Describe(EffectDefinition effect, int rank, int maxRank, Unit caster, StringBuilder body, HashSet<object> seen, int depth)
        {
            if (effect == null || depth > 3) return;
            switch (effect)
            {
                case DamageEffect damage:
                    body.Append("\n<color=#e07050>Dégâts ").Append(DamageName(damage.Type)).Append("</color> ").Append(Value(damage.Amount, rank, maxRank, caster));
                    break;
                case HealEffect heal:
                    body.Append("\n<color=#70d070>Soin</color> ").Append(Value(heal.Amount, rank, maxRank, caster));
                    break;
                case ShieldEffect shield:
                    body.Append("\n<color=#d0d0f0>Bouclier</color> ").Append(Value(shield.Amount, rank, maxRank, caster)).Append(", ").Append(Number(shield.DurationSecondsAt(rank))).Append(" s");
                    break;
                case ApplyStatusEffect status when status.Status != null:
                    body.Append("\n<color=#a0c0ff>").Append(status.Status.DisplayName).Append("</color> ").Append(Ranked(r => status.DurationSecondsAt(r), maxRank, rank, true)).Append(" s");
                    foreach (RankedStatModifierData modifier in status.ModifierOverrides)
                        body.Append(", ").Append(StatName(modifier.Stat)).Append(' ').Append(Ranked(r => modifier.Value.At(r) * (modifier.Kind == ModifierKind.Flat ? 1f : 100f), maxRank, rank, true)).Append(modifier.Kind == ModifierKind.Flat ? "" : " %");
                    break;
                case CreateAreaEffect create when create.Area != null && seen.Add(create.Area):
                    if (create.Area.Shape == AreaShape.Circle) body.Append("\n<color=#c8aa6e>Zone</color> rayon ").Append(Ranked(r => create.Area.RadiusAt(r), maxRank, rank, true)).Append(" m");
                    foreach (EffectDefinition inner in create.Area.OnActivate) Describe(inner, rank, maxRank, caster, body, seen, depth + 1);
                    foreach (EffectDefinition inner in create.Area.OnInterval) Describe(inner, rank, maxRank, caster, body, seen, depth + 1);
                    break;
                case LaunchProjectileEffect launch when launch.Projectile != null && seen.Add(launch.Projectile):
                    foreach (EffectDefinition inner in launch.Projectile.OnHit) Describe(inner, rank, maxRank, caster, body, seen, depth + 1);
                    break;
            }
        }

        private static string Value(ScalingValueData value, int rank, int maxRank, Unit caster)
        {
            var text = new StringBuilder(Ranked(r => BaseAt(value, r), maxRank, rank, true));
            foreach (ScalingRatio ratio in value.Ratios)
            {
                float factor = ratio.RatioAt(rank);
                if (factor == 0f) continue;
                text.Append(" <color=#70e070>(+").Append(Number(factor * 100f)).Append(" % ").Append(SourceName(ratio)).Append(')');
                float current = SourceValue(ratio, caster);
                if (current > 0f) text.Append(" = +").Append(Number(factor * current));
                text.Append("</color>");
            }
            return text.ToString();
        }

        private static float BaseAt(ScalingValueData value, int rank)
        {
            IReadOnlyList<float> bases = value.BaseByRank;
            if (bases == null || bases.Count == 0) return 0f;
            return bases[System.Math.Min(System.Math.Max(rank, 1), bases.Count) - 1];
        }

        private static float SourceValue(in ScalingRatio ratio, Unit caster)
        {
            if (caster == null) return 0f;
            switch (ratio.Source)
            {
                case ScalingSource.CasterTotalStat: return caster.Stats.Get(ratio.Stat);
                case ScalingSource.CasterBonusStat: return caster.Stats.GetBonus(ratio.Stat);
                case ScalingSource.CasterBaseStat: return caster.Stats.GetBase(ratio.Stat);
                default: return 0f;
            }
        }

        // "80/120/160" with the current rank highlighted; a single value when every rank is the same.
        private static string Ranked(System.Func<int, float> at, int maxRank, int rank, bool detailed)
        {
            bool constant = true;
            for (int r = 2; r <= maxRank; r++) if (at(r) != at(1)) constant = false;
            if (constant || !detailed) return Number(at(rank));
            var text = new StringBuilder();
            for (int r = 1; r <= maxRank; r++)
            {
                if (r > 1) text.Append('/');
                string value = Number(at(r));
                text.Append(r == rank ? $"<b><color=#ffffff>{value}</color></b>" : value);
            }
            return text.ToString();
        }

        private static string SourceName(in ScalingRatio ratio)
        {
            switch (ratio.Source)
            {
                case ScalingSource.CasterBonusStat: return StatName(ratio.Stat) + " bonus";
                case ScalingSource.CasterBaseStat: return StatName(ratio.Stat) + " de base";
                case ScalingSource.TargetMaxHealth: return "des PV max de la cible";
                case ScalingSource.TargetMissingHealth: return "des PV manquants de la cible";
                case ScalingSource.TargetCurrentHealth: return "des PV actuels de la cible";
                case ScalingSource.Stacks: return "par cumul";
                default: return StatName(ratio.Stat);
            }
        }

        private static string DamageName(DamageType type) => type == DamageType.Physical ? "physiques" : type == DamageType.Magical ? "magiques" : "bruts";

        private static string StatValue(StatModifierData stat) =>
            stat.Kind == ModifierKind.Flat ? Number(stat.Value) : Number(stat.Value * 100f) + " %";

        public static string StatName(StatType stat)
        {
            switch (stat)
            {
                case StatType.MaxHealth: return "PV";
                case StatType.HealthRegen: return "régén. PV";
                case StatType.MaxMana: return "mana";
                case StatType.ManaRegen: return "régén. mana";
                case StatType.AttackDamage: return "AD";
                case StatType.AbilityPower: return "AP";
                case StatType.Armor: return "armure";
                case StatType.MagicResist: return "RM";
                case StatType.AttackSpeed: return "vitesse d'attaque";
                case StatType.MoveSpeed: return "vitesse";
                case StatType.AttackRange: return "portée";
                case StatType.CritChance: return "critique";
                case StatType.AbilityHaste: return "accélération";
                case StatType.LifeSteal: return "vol de vie";
                case StatType.VisionRange: return "vision";
                default: return stat.ToString();
            }
        }

        private static string Number(float value) => value.ToString(System.Math.Abs(value - System.Math.Round(value)) < 0.01f ? "0" : "0.##", French);
    }
}
