using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Economy;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Core.Simulation;
using MangaLegends.Data.Serialization;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Champions
{
    public sealed class Inventory : NetworkBehaviour, IStatModifierSource
    {
        public const int Capacity = 8;
        private const int SourceBase = 2000;

        private readonly List<IItemRecipe> _owned = new List<IItemRecipe>();
        private Unit _unit;
        private ChampionProgress _progress;

        [Networked, Capacity(Capacity)] public NetworkArray<short> Items => default;
        [Networked] public int Version { get; set; }
        [Networked, Capacity(Capacity)] public NetworkArray<int> ActiveReadyTicks => default;

        public int ModifierVersion => Version;
        public int SlotCount => _unit != null ? System.Math.Min(Capacity, _unit.Services.Rules.InventorySlots) : 0;

        public override void Spawned()
        {
            _unit = GetComponent<Unit>();
            _progress = GetComponent<ChampionProgress>();
        }

        public ItemDefinition ItemAt(int slot)
        {
            if (_unit == null || slot < 0 || slot >= Capacity) return null;
            int index = Items[slot] - 1;
            var items = _unit.Services.Content.Items;
            return index >= 0 && index < items.Count ? items[index] : null;
        }

        public PurchaseQuote Quote(ItemDefinition item) =>
            ItemPricing.Quote(item, Owned(), SlotCount, _progress != null ? _progress.GoldAmount : 0);

        public bool CanShop => _unit != null && _unit.Services.Match != null && _unit.Services.Match.IsInShopRange(_unit);

        public bool TryBuy(int itemIndex)
        {
            if (!HasStateAuthority || !CanShop) return false;
            var items = _unit.Services.Content.Items;
            if (itemIndex < 0 || itemIndex >= items.Count) return false;
            ItemDefinition item = items[itemIndex];
            PurchaseQuote quote = Quote(item);
            if (!quote.CanBuy || !_progress.SpendGold(quote.Cost)) return false;
            foreach (int consumed in quote.ConsumedSlots) Items.Set(consumed, 0);
            int free = FirstFree();
            if (free < 0) return false;
            Items.Set(free, (short)(itemIndex + 1));
            Version++;
            return true;
        }

        public bool TrySell(int slot)
        {
            if (!HasStateAuthority || !CanShop) return false;
            ItemDefinition item = ItemAt(slot);
            if (item == null) return false;
            Items.Set(slot, 0);
            _progress.AddGold(ItemPricing.SellValue(item, _unit.Services.Rules.SellRatio));
            Version++;
            return true;
        }

        public bool IsActiveReady(int slot) => slot >= 0 && slot < Capacity && Runner.Tick >= ActiveReadyTicks[slot];

        public float ActiveCooldownRemaining(int slot) =>
            slot >= 0 && slot < Capacity ? Mathf.Max(0f, (ActiveReadyTicks[slot] - Runner.Tick) / (float)Runner.TickRate) : 0f;

        public bool TryUseActive(int slot, Vector3 aimPoint, Unit aimTarget)
        {
            ItemDefinition item = ItemAt(slot);
            if (item == null || !item.Active.Enabled || !_unit.IsAlive || !_unit.Status.CanCast || !IsActiveReady(slot)) return false;
            TargetingData targeting = item.Active.Targeting;
            float range = targeting.RangeAt(1);
            Unit target = null;
            switch (targeting.Mode)
            {
                case TargetingMode.Unit:
                    if (aimTarget == null || !aimTarget.IsAlive || !aimTarget.IsTargetable || !TeamRules.Matches(targeting.ValidTargets, _unit, aimTarget)) return false;
                    if (UnitRegistry.FlatDistance(_unit.Position, aimTarget.Position) - aimTarget.Radius - _unit.Radius > range) return false;
                    target = aimTarget;
                    aimPoint = target.Position;
                    break;
                case TargetingMode.Self:
                case TargetingMode.None:
                    target = _unit;
                    aimPoint = _unit.Position;
                    break;
                case TargetingMode.Point:
                    Vector3 offset = aimPoint - _unit.Position;
                    offset.y = 0f;
                    aimPoint = _unit.Position + Vector3.ClampMagnitude(offset, range);
                    break;
            }
            ActiveReadyTicks.Set(slot, Runner.Tick + TickTime.SecondsToTicks(item.Active.CooldownSeconds, Runner.TickRate));
            _unit.Motor?.Face(aimPoint);
            EffectExecutor.Run(item.Active.Effects, new EffectContext(_unit, target, aimPoint, _unit.Position, 1, null));
            return true;
        }

        public void CollectModifiers(List<StatModifier> into)
        {
            for (int slot = 0; slot < Capacity; slot++)
            {
                ItemDefinition item = ItemAt(slot);
                if (item == null) continue;
                foreach (StatModifierData modifier in item.Stats) into.Add(modifier.ToModifier(SourceBase + slot));
            }
        }

        private List<IItemRecipe> Owned()
        {
            _owned.Clear();
            for (int slot = 0; slot < SlotCount; slot++) _owned.Add(ItemAt(slot));
            return _owned;
        }

        private int FirstFree()
        {
            for (int slot = 0; slot < SlotCount; slot++)
                if (Items[slot] == 0) return slot;
            return -1;
        }
    }
}
