using Fusion;
using MangaLegends.Simulation.Units;

namespace MangaLegends.Simulation.AI
{
    // A ward does nothing but see; it remembers who placed it and when it runs out.
    public sealed class WardBrain : NetworkBehaviour, IUnitBrain
    {
        private Unit _unit;

        [Networked] public NetworkId Owner { get; set; }
        [Networked] public int PlacedTick { get; set; }
        [Networked] public int ExpireTick { get; set; }

        private void Awake() => _unit = GetComponent<Unit>();

        public void Think()
        {
            if (HasStateAuthority && ExpireTick > 0 && Runner.Tick >= ExpireTick) _unit.Health.Expire();
        }
    }
}
