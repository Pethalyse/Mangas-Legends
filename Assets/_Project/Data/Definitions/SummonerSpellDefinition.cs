using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Summoner Spell", fileName = "SPELL_New")]
    public sealed class SummonerSpellDefinition : ContentDefinition
    {
        [SerializeField] private AbilityDefinition _ability;

        public AbilityDefinition Ability => _ability;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (_ability == null) report.Error(Id, "Summoner spell has no ability.");
        }
    }
}
