using System;
using System.Collections.Generic;

namespace MangaLegends.Core.Economy
{
    public static class ItemPricing
    {
        public static int TotalCost(IItemRecipe item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            int total = item.RecipeCost;
            foreach (IItemRecipe component in item.Components) total += TotalCost(component);
            return total;
        }

        public static int SellValue(IItemRecipe item, float sellRatio) =>
            (int)Math.Floor(TotalCost(item) * sellRatio);

        public static PurchaseQuote Quote(IItemRecipe item, IReadOnlyList<IItemRecipe> inventory, int maxSlots, int gold)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            inventory ??= Array.Empty<IItemRecipe>();

            if (item.IsUnique && Contains(inventory, item.Id))
                return new PurchaseQuote(0, Array.Empty<int>(), PurchaseFailure.UniqueAlreadyOwned);

            List<int> consumed = new List<int>();
            bool[] used = new bool[inventory.Count];
            int cost = item.RecipeCost;
            foreach (IItemRecipe component in item.Components) cost += CostAfterOwned(component, inventory, used, consumed);

            int occupied = 0;
            foreach (IItemRecipe slot in inventory)
                if (slot != null) occupied++;
            bool hasRoom = occupied - consumed.Count < maxSlots;

            consumed.Sort();
            PurchaseFailure failure = !hasRoom ? PurchaseFailure.InventoryFull
                : cost > gold ? PurchaseFailure.NotEnoughGold
                : PurchaseFailure.None;
            return new PurchaseQuote(cost, consumed, failure);
        }

        private static int CostAfterOwned(IItemRecipe component, IReadOnlyList<IItemRecipe> inventory, bool[] used, List<int> consumed)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (used[i] || inventory[i] == null || inventory[i].Id != component.Id) continue;
                used[i] = true;
                consumed.Add(i);
                return 0;
            }

            int cost = component.RecipeCost;
            foreach (IItemRecipe sub in component.Components) cost += CostAfterOwned(sub, inventory, used, consumed);
            return cost;
        }

        private static bool Contains(IReadOnlyList<IItemRecipe> inventory, string id)
        {
            foreach (IItemRecipe owned in inventory)
                if (owned != null && owned.Id == id) return true;
            return false;
        }
    }
}
