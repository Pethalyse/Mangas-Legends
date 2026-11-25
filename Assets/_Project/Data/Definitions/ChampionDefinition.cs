using System.Collections.Generic;
using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Champion", fileName = "CHAMP_New")]
    public sealed class ChampionDefinition : UnitDefinition
    {
        [SerializeField] private ChampionRole _role;
        [SerializeField] private ResourceKind _resource = ResourceKind.Mana;
        [SerializeField] private ChampionTraits _traits;
        [SerializeField] private AbilitySet _defaultAbilities = new AbilitySet();
        [SerializeField] private List<AbilitySet> _alternateAbilitySets = new List<AbilitySet>();

        public ChampionRole Role => _role;
        public ResourceKind Resource => _resource;
        public ChampionTraits Traits => _traits;
        public bool Has(ChampionTraits trait) => (_traits & trait) == trait;
        public AbilitySet DefaultAbilities => _defaultAbilities;
        public IReadOnlyList<AbilitySet> AlternateAbilitySets => _alternateAbilitySets;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (Kind != UnitKind.Champion) report.Error(Id, "A champion definition must have Kind = Champion.");
            _defaultAbilities.Validate(Id, report, allowEmptySlots: false);
            foreach (AbilitySet set in _alternateAbilitySets) set.Validate(Id, report, allowEmptySlots: true);
        }
    }
}
