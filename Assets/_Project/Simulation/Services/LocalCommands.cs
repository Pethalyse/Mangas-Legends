namespace MangaLegends.Simulation.Services
{
    // One-shot requests from the local UI, consumed by the input collector on the next tick.
    public sealed class LocalCommands
    {
        private int _buyItem = -1;
        private int _sellSlot = -1;
        private int _levelUpSlot = -1;

        public void RequestBuy(int itemIndex) => _buyItem = itemIndex;
        public void RequestSell(int inventorySlot) => _sellSlot = inventorySlot;
        public void RequestLevelUp(int abilitySlot) => _levelUpSlot = abilitySlot;

        public int TakeBuy() => Take(ref _buyItem);
        public int TakeSell() => Take(ref _sellSlot);
        public int TakeLevelUp() => Take(ref _levelUpSlot);

        private static int Take(ref int value)
        {
            int taken = value;
            value = -1;
            return taken;
        }
    }
}
