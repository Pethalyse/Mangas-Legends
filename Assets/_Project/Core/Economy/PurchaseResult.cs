using System.Collections.Generic;

namespace MangaLegends.Core.Economy
{
    public enum PurchaseFailure
    {
        None,
        NotEnoughGold,
        InventoryFull,
        UniqueAlreadyOwned
    }

    public readonly struct PurchaseQuote
    {
        public PurchaseQuote(int cost, IReadOnlyList<int> consumedSlots, PurchaseFailure failure)
        {
            Cost = cost;
            ConsumedSlots = consumedSlots;
            Failure = failure;
        }

        public int Cost { get; }
        public IReadOnlyList<int> ConsumedSlots { get; }
        public PurchaseFailure Failure { get; }
        public bool CanBuy => Failure == PurchaseFailure.None;
    }
}
