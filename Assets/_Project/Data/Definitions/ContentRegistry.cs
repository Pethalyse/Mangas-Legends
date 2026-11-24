using System.Collections.Generic;
using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Content Registry", fileName = "CONTENT_Registry")]
    public sealed class ContentRegistry : ScriptableObject
    {
        [SerializeField] private GameRulesConfig _rules;
        [SerializeField] private List<ContentDefinition> _entries = new List<ContentDefinition>();

        private ContentIndex<ChampionDefinition> _champions;
        private ContentIndex<UnitDefinition> _units;
        private ContentIndex<AbilityDefinition> _abilities;
        private ContentIndex<StatusEffectDefinition> _statusEffects;
        private ContentIndex<ProjectileDefinition> _projectiles;
        private ContentIndex<AreaDefinition> _areas;
        private ContentIndex<ItemDefinition> _items;

        public GameRulesConfig Rules => _rules;
        public IReadOnlyList<ContentDefinition> Entries => _entries;
        public ContentIndex<ChampionDefinition> Champions => Built(ref _champions);
        public ContentIndex<UnitDefinition> Units => Built(ref _units);
        public ContentIndex<AbilityDefinition> Abilities => Built(ref _abilities);
        public ContentIndex<StatusEffectDefinition> StatusEffects => Built(ref _statusEffects);
        public ContentIndex<ProjectileDefinition> Projectiles => Built(ref _projectiles);
        public ContentIndex<AreaDefinition> Areas => Built(ref _areas);
        public ContentIndex<ItemDefinition> Items => Built(ref _items);

        public ValidationReport Validate()
        {
            ValidationReport report = new ValidationReport();
            if (_rules == null) report.Error(name, "No GameRulesConfig assigned.");
            report.CheckIds(_entries);
            foreach (ContentDefinition entry in _entries)
            {
                if (entry == null) report.Error(name, "The registry contains an empty entry.");
                else entry.Validate(report);
            }
            return report;
        }

        public void Rebuild()
        {
            _champions = new ContentIndex<ChampionDefinition>(OfType<ChampionDefinition>());
            _units = new ContentIndex<UnitDefinition>(OfType<UnitDefinition>());
            _abilities = new ContentIndex<AbilityDefinition>(OfType<AbilityDefinition>());
            _statusEffects = new ContentIndex<StatusEffectDefinition>(OfType<StatusEffectDefinition>());
            _projectiles = new ContentIndex<ProjectileDefinition>(OfType<ProjectileDefinition>());
            _areas = new ContentIndex<AreaDefinition>(OfType<AreaDefinition>());
            _items = new ContentIndex<ItemDefinition>(OfType<ItemDefinition>());
        }

#if UNITY_EDITOR
        public void SetEntries(GameRulesConfig rules, List<ContentDefinition> entries)
        {
            _rules = rules;
            _entries = entries;
            _champions = null;
        }
#endif

        private void OnEnable() => _champions = null;

        private ContentIndex<T> Built<T>(ref ContentIndex<T> index) where T : class, IContentEntry
        {
            if (_champions == null) Rebuild();
            return index;
        }

        private IEnumerable<T> OfType<T>() where T : ContentDefinition
        {
            foreach (ContentDefinition entry in _entries)
                if (entry is T typed) yield return typed;
        }
    }
}
