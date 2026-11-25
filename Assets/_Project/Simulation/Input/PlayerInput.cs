using Fusion;
using UnityEngine;

namespace MangaLegends.Simulation.Input
{
    public enum InputButton
    {
        Move,
        AttackMove,
        Stop,
        AbilityA,
        AbilityZ,
        AbilityE,
        AbilityR,
        Summoner1,
        Summoner2,
        Recall,
        Item1,
        Item2,
        Item3,
        Item4,
        Item5,
        Item6,
        Trinket
    }

    // Each set button is an event for this tick; the collector accumulates presses between ticks.
    public struct PlayerInput : INetworkInput
    {
        public NetworkButtons Buttons;
        public Vector3 MovePoint;
        public NetworkId MoveTarget;
        public Vector3 AimPoint;
        public NetworkId AimTarget;
        public short BuyItem;
        public sbyte SellSlot;
        public sbyte LevelUpSlot;

        public bool Has(InputButton button) => Buttons.IsSet(button);

        public static PlayerInput Empty => new PlayerInput { BuyItem = -1, SellSlot = -1, LevelUpSlot = -1 };
    }
}
