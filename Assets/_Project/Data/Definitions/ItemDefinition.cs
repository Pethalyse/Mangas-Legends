using System;
using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Core.Economy;
using MangaLegends.Data.Effects;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    public enum ItemTier
    {
        Basic,
        Component,
        Complete,
        Consumable
    }

    public enum ItemTrigger
    {
        OnHit,
        OnAbilityHit,
        OnHeal,
        OnKill,
        OnDamageTaken,
        Periodic
    }

    [CreateAssetMenu(menuName = "MangaLegends/Item", fileName = "ITEM_New")]
    public sealed class ItemDefinition : ContentDefinition, IItemRecipe
    {
        [SerializeField] private ItemTier _tier;
        [SerializeField, Min(0)] private int _recipeCost;
        [SerializeField] private bool _unique;
        [SerializeField] private List<ItemDefinition> _components = new List<ItemDefinition>();
        [SerializeField] private List<StatModifierData> _stats = new List<StatModifierData>();
        [SerializeField] private List<ItemPassive> _passives = new List<ItemPassive>();
        [SerializeField] private ItemActive _active = new ItemActive();

        public ItemTier Tier => _tier;
        public int RecipeCost => _recipeCost;
        public bool IsUnique => _unique;
        public IReadOnlyList<IItemRecipe> Components => _components;
        public IReadOnlyList<ItemDefinition> ComponentDefinitions => _components;
        public IReadOnlyList<StatModifierData> Stats => _stats;
        public IReadOnlyList<ItemPassive> Passives => _passives;
        public ItemActive Active => _active;
        public int TotalCost => ItemPricing.TotalCost(this);

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            foreach (ItemDefinition component in _components)
            {
                if (component == null) report.Error(Id, "A component entry is empty.");
                else if (component == this || DependsOn(component, this, 0)) report.Error(Id, "Recipe is circular.");
            }
            foreach (ItemPassive passive in _passives) passive.Validate(Id, report);
            _active.Validate(Id, report);
        }

        private static bool DependsOn(ItemDefinition item, ItemDefinition target, int depth)
        {
            if (depth > 16) return true;
            foreach (ItemDefinition component in item._components)
                if (component == target || (component != null && DependsOn(component, target, depth + 1))) return true;
            return false;
        }
    }

    [Serializable]
    public sealed class ItemPassive
    {
        [SerializeField] private string _name;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private ItemTrigger _trigger;
        [SerializeField, Min(0f)] private float _cooldownSeconds;
        [SerializeField] private bool _uniqueEffect = true;
        [SerializeReference, TypeSelector] private List<EffectDefinition> _effects = new List<EffectDefinition>();

        public string Name => _name;
        public string Description => _description;
        public ItemTrigger Trigger => _trigger;
        public float CooldownSeconds => _cooldownSeconds;
        public bool UniqueEffect => _uniqueEffect;
        public IReadOnlyList<EffectDefinition> Effects => _effects;

        public void Validate(string ownerId, ValidationReport report)
        {
            if (_effects.Count == 0) report.Warning(ownerId, $"Passive '{_name}' has no effect.");
            foreach (EffectDefinition effect in _effects) effect?.Validate(ownerId, 1, report);
        }
    }

    [Serializable]
    public sealed class ItemActive
    {
        [SerializeField] private bool _enabled;
        [SerializeField, Tooltip("The item disappears once its active is used.")] private bool _consumedOnUse;
        [SerializeField] private string _name;
        [SerializeField, TextArea] private string _description;
        [SerializeField, Min(0f)] private float _cooldownSeconds;
        [SerializeField] private TargetingData _targeting = new TargetingData();
        [SerializeReference, TypeSelector] private List<EffectDefinition> _effects = new List<EffectDefinition>();

        public bool Enabled => _enabled;
        public bool ConsumedOnUse => _consumedOnUse;
        public string Name => _name;
        public string Description => _description;
        public float CooldownSeconds => _cooldownSeconds;
        public TargetingData Targeting => _targeting;
        public IReadOnlyList<EffectDefinition> Effects => _effects;

        public void Validate(string ownerId, ValidationReport report)
        {
            if (!_enabled) return;
            if (_effects.Count == 0) report.Warning(ownerId, $"Active '{_name}' has no effect.");
            foreach (EffectDefinition effect in _effects) effect?.Validate(ownerId, 1, report);
        }
    }
}
