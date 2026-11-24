using System;
using System.Collections.Generic;
using MangaLegends.Core.Economy;
using NUnit.Framework;

namespace MangaLegends.Tests.EditMode
{
    public sealed class ItemPricingTests
    {
        private sealed class Item : IItemRecipe
        {
            public Item(string id, int recipeCost, bool unique = false, params IItemRecipe[] components)
            {
                Id = id;
                RecipeCost = recipeCost;
                IsUnique = unique;
                Components = components;
            }

            public string Id { get; }
            public int RecipeCost { get; }
            public bool IsUnique { get; }
            public IReadOnlyList<IItemRecipe> Components { get; }
        }

        private static readonly Item Blade = new Item("item.blade", 350);
        private static readonly Item Dagger = new Item("item.dagger", 300);
        private static readonly Item Ore = new Item("item.strange_ore", 770);
        private static readonly Item SteelBlade = new Item("item.steel_blade", 150, false, Blade, Blade, Dagger);
        private static readonly Item Stone = new Item("item.reinforcement_stone", 330, false, Ore);
        private static readonly Item Katana = new Item("item.legendary_katana", 750, true, SteelBlade, Stone, Dagger);

        [Test]
        public void TotalCost_SumsRecipeAndComponentsRecursively()
        {
            Assert.That(ItemPricing.TotalCost(SteelBlade), Is.EqualTo(1150));
            Assert.That(ItemPricing.TotalCost(Katana), Is.EqualTo(3300));
        }

        [Test]
        public void Quote_DeductsOwnedComponents_IncludingNestedOnes()
        {
            List<IItemRecipe> inventory = new List<IItemRecipe> { Blade, Ore, Dagger };

            PurchaseQuote quote = ItemPricing.Quote(Katana, inventory, maxSlots: 6, gold: 10000);

            Assert.That(quote.Cost, Is.EqualTo(3300 - 350 - 770 - 300));
            Assert.That(quote.ConsumedSlots, Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(quote.CanBuy, Is.True);
        }

        [Test]
        public void Quote_PrefersWholeComponentOverItsParts()
        {
            List<IItemRecipe> inventory = new List<IItemRecipe> { SteelBlade, Blade };

            PurchaseQuote quote = ItemPricing.Quote(Katana, inventory, 6, 10000);

            Assert.That(quote.ConsumedSlots, Is.EqualTo(new[] { 0 }));
            Assert.That(quote.Cost, Is.EqualTo(3300 - 1150));
        }

        [Test]
        public void Quote_AllowsFullInventoryWhenAComponentFreesASlot()
        {
            List<IItemRecipe> full = new List<IItemRecipe> { Blade, Blade, Blade, Blade, Blade, Dagger };

            Assert.That(ItemPricing.Quote(Stone, full, 6, 10000).Failure, Is.EqualTo(PurchaseFailure.InventoryFull));
            Assert.That(ItemPricing.Quote(SteelBlade, full, 6, 10000).CanBuy, Is.True);
        }

        [Test]
        public void Quote_RejectsMissingGoldAndDuplicateUnique()
        {
            Assert.That(ItemPricing.Quote(Katana, Array.Empty<IItemRecipe>(), 6, 100).Failure, Is.EqualTo(PurchaseFailure.NotEnoughGold));
            Assert.That(ItemPricing.Quote(Katana, new IItemRecipe[] { Katana }, 6, 10000).Failure, Is.EqualTo(PurchaseFailure.UniqueAlreadyOwned));
        }

        [Test]
        public void SellValue_UsesRatioOfTotalCost()
        {
            Assert.That(ItemPricing.SellValue(SteelBlade, 0.7f), Is.EqualTo(805));
        }
    }
}
