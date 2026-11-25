using System.Collections.Generic;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Effects;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Input;
using MangaLegends.Simulation.Units;

namespace MangaLegends.Simulation.Abilities
{
    // What a cast key would use right now: read by the input collector to pick a cast flow and by the aim indicator to draw it.
    public readonly struct AimSource
    {
        public AimSource(TargetingData targeting, IReadOnlyList<EffectDefinition> effects, int rank, bool ready)
        {
            Targeting = targeting;
            Effects = effects;
            Rank = rank < 1 ? 1 : rank;
            Ready = ready;
        }

        public TargetingData Targeting { get; }
        public IReadOnlyList<EffectDefinition> Effects { get; }
        public int Rank { get; }
        public bool Ready { get; }
        public bool IsValid => Targeting != null;
        public bool NeedsAim => IsValid && (Targeting.Mode == TargetingMode.Unit || Targeting.Mode == TargetingMode.Point || Targeting.Mode == TargetingMode.Direction);

        public static bool TryCasterSlot(InputButton button, out CasterSlot slot)
        {
            switch (button)
            {
                case InputButton.AbilityA: slot = CasterSlot.A; return true;
                case InputButton.AbilityZ: slot = CasterSlot.Z; return true;
                case InputButton.AbilityE: slot = CasterSlot.E; return true;
                case InputButton.AbilityR: slot = CasterSlot.R; return true;
                case InputButton.Summoner1: slot = CasterSlot.Summoner1; return true;
                case InputButton.Summoner2: slot = CasterSlot.Summoner2; return true;
                case InputButton.Recall: slot = CasterSlot.Recall; return true;
                default: slot = CasterSlot.Passive; return false;
            }
        }

        public static int ItemSlotOf(InputButton button) =>
            button >= InputButton.Item1 && button <= InputButton.Item6 ? button - InputButton.Item1 : -1;

        public static AimSource For(Unit champion, InputButton button)
        {
            if (champion == null || champion.Object == null || !champion.IsAlive) return default;
            if (TryCasterSlot(button, out CasterSlot slot) && champion.Caster != null)
            {
                AbilityDefinition ability = champion.Caster.AbilityAt(slot);
                if (ability == null || ability.CastMode == CastMode.Passive || champion.Caster.RankOf(slot) == 0) return default;
                return new AimSource(ability.Targeting, ability.OnCast, champion.Caster.RankOf(slot), champion.Caster.IsReady(slot));
            }
            int itemSlot = ItemSlotOf(button);
            Inventory inventory = champion.GetComponent<Inventory>();
            if (itemSlot < 0 || inventory == null) return default;
            ItemDefinition item = inventory.ItemAt(itemSlot);
            if (item == null || !item.Active.Enabled) return default;
            return new AimSource(item.Active.Targeting, item.Active.Effects, 1, inventory.IsActiveReady(itemSlot));
        }
    }
}
