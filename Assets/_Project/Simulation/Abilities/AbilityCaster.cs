using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Abilities;
using MangaLegends.Core.Simulation;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Abilities
{
    public enum CasterSlot
    {
        Passive,
        A,
        Z,
        E,
        R,
        Summoner1,
        Summoner2,
        Recall,
        Trinket
    }

    public struct NetSlot : INetworkStruct
    {
        public short Ability;
        public byte Rank;
        public byte Charges;
        public int ReadyTick;
        public int ChargeTick;
    }

    [RequireComponent(typeof(ProjectileLauncher), typeof(AreaSpawner))]
    public sealed class AbilityCaster : NetworkBehaviour
    {
        public const int SlotCount = 9;
        public const int NoAbility = -1;

        private Unit _unit;
        private readonly List<Unit> _allies = new List<Unit>();

        [Networked, Capacity(SlotCount)] public NetworkArray<NetSlot> Slots => default;
        [Networked] public byte ActiveSet { get; set; }
        [Networked] public int SetEndTick { get; set; }
        [Networked] public sbyte ChannelSlot { get; set; }
        [Networked] public int ChannelStartTick { get; set; }
        [Networked] public int ChannelEndTick { get; set; }
        [Networked] public Vector3 ChannelPoint { get; set; }
        [Networked] public NetworkId ChannelTarget { get; set; }
        [Networked] public sbyte PendingSlot { get; set; }
        [Networked] public NetworkId PendingTarget { get; set; }
        [Networked] public int LastCastTick { get; set; }
        [Networked] public sbyte LastCastSlot { get; set; }
        [Networked] public short BaseTrinket { get; set; }
        [Networked] public int TrinketOverrideEndTick { get; set; }
        [Networked] public int TrinketSavedReadyTick { get; set; }

        public ProjectileLauncher Projectiles { get; private set; }
        public AreaSpawner Areas { get; private set; }
        public bool IsChanneling => ChannelSlot >= 0;

        private void Awake()
        {
            Projectiles = GetComponent<ProjectileLauncher>();
            Areas = GetComponent<AreaSpawner>();
        }

        public void Initialize(Unit unit)
        {
            _unit = unit;
            Projectiles.Initialize(unit);
            Areas.Initialize(unit);
            if (!HasStateAuthority) return;
            ChannelSlot = -1;
            PendingSlot = -1;
            LastCastSlot = -1;
            for (int i = 0; i < SlotCount; i++) Slots.Set(i, new NetSlot { Ability = NoAbility });
        }

        public void Setup(ChampionDefinition champion, IReadOnlyList<SummonerSpellDefinition> summoners, AbilityDefinition recall, AbilityDefinition trinket)
        {
            if (!HasStateAuthority) return;
            ApplySet(champion.DefaultAbilities, keepRanks: false);
            SetSlotAbility(CasterSlot.Passive, champion.DefaultAbilities.Get(AbilitySlot.Passive), 1);
            for (int i = 0; i < 2; i++)
            {
                AbilityDefinition spell = summoners != null && i < summoners.Count && summoners[i] != null ? summoners[i].Ability : null;
                SetSlotAbility(CasterSlot.Summoner1 + i, spell, 1);
            }
            SetSlotAbility(CasterSlot.Recall, recall, 1);
            SetSlotAbility(CasterSlot.Trinket, trinket, 1);
            BaseTrinket = Slots[(int)CasterSlot.Trinket].Ability;
        }

        public AbilityDefinition AbilityAt(CasterSlot slot)
        {
            short index = Slots[(int)slot].Ability;
            var abilities = _unit.Services.Content.Abilities;
            return index >= 0 && index < abilities.Count ? abilities[index] : null;
        }

        public int RankOf(CasterSlot slot) => Slots[(int)slot].Rank;

        public float CooldownRemaining(CasterSlot slot) =>
            Mathf.Max(0f, (Slots[(int)slot].ReadyTick - Runner.Tick) / (float)Runner.TickRate);

        public float CooldownTotal(CasterSlot slot)
        {
            AbilityDefinition ability = AbilityAt(slot);
            if (ability == null) return 0f;
            return CooldownCalculator.ApplyHaste(ability.CooldownAtRank(Mathf.Max(1, RankOf(slot))), _unit.Stats.Get(StatType.AbilityHaste), _unit.Services.Rules.AbilityHasteScale);
        }

        public int SkillPoints
        {
            get
            {
                int spent = 0;
                for (int i = (int)CasterSlot.A; i <= (int)CasterSlot.R; i++) spent += Slots[i].Rank;
                return Mathf.Max(0, _unit.Stats.Level - spent);
            }
        }

        public bool CanLevelUp(CasterSlot slot)
        {
            if (slot < CasterSlot.A || slot > CasterSlot.R || SkillPoints <= 0) return false;
            AbilityDefinition ability = AbilityAt(slot);
            if (ability == null) return false;
            int rank = RankOf(slot);
            GameRulesConfig rules = _unit.Services.Rules;
            if (rank >= ability.MaxRank) return false;
            if (slot == CasterSlot.R)
            {
                int[] unlocks = rules.UltimateUnlockLevels;
                return rank < unlocks.Length && _unit.Stats.Level >= unlocks[rank];
            }
            return rank < rules.BasicAbilityMaxRank && rank < (_unit.Stats.Level + 1) / 2;
        }

        public void LevelUp(CasterSlot slot)
        {
            if (!CanLevelUp(slot)) return;
            NetSlot data = Slots[(int)slot];
            data.Rank++;
            if (data.Rank == 1) data.Charges = (byte)AbilityAt(slot).ChargesAt(1);
            Slots.Set((int)slot, data);
        }

        public bool IsReady(CasterSlot slot)
        {
            NetSlot data = Slots[(int)slot];
            AbilityDefinition ability = AbilityAt(slot);
            if (ability == null || data.Rank == 0) return false;
            return ability.ChargesAt(data.Rank) > 1 ? data.Charges > 0 : Runner.Tick >= data.ReadyTick;
        }

        public bool TryCast(CasterSlot slot, Vector3 aimPoint, Unit aimTarget)
        {
            AbilityDefinition ability = AbilityAt(slot);
            if (ability == null || ability.CastMode == CastMode.Passive || !_unit.IsAlive || IsChanneling) return false;
            bool summoner = slot >= CasterSlot.Summoner1;
            if (!summoner && !_unit.Status.CanCast) return false;
            if (summoner && !_unit.Status.CanMove && ability.CastMode == CastMode.Instant && HasDash(ability)) return false;
            if (!IsReady(slot)) return false;
            int rank = Mathf.Max(1, RankOf(slot));
            float cost = ability.CostAtRank(rank);
            if (_unit.Resource != null && !_unit.Resource.CanAfford(cost)) return false;

            if (ability.CastMode == CastMode.Toggle) return Toggle(slot, ability, rank, cost);

            TargetingData targeting = ability.Targeting;
            float range = targeting.RangeAt(rank);
            Unit target = null;
            switch (targeting.Mode)
            {
                case TargetingMode.Unit:
                    target = ResolveUnitTarget(targeting, aimTarget, range);
                    if (target == null) return false;
                    if (!InRange(target, range))
                    {
                        PendingSlot = (sbyte)slot;
                        PendingTarget = target.Object.Id;
                        _unit.Motor?.MoveTo(target.Position, range + target.Radius + _unit.Radius - 0.2f);
                        return false;
                    }
                    aimPoint = target.Position;
                    break;
                case TargetingMode.Self:
                    target = _unit;
                    aimPoint = _unit.Position;
                    break;
                case TargetingMode.Point:
                    Vector3 offset = aimPoint - _unit.Position;
                    offset.y = 0f;
                    aimPoint = _unit.Position + Vector3.ClampMagnitude(offset, range);
                    break;
            }

            Commit(slot, ability, rank, cost);
            _unit.Motor?.Face(aimPoint);
            if (ability.CastMode == CastMode.Channel)
            {
                StartChannel(slot, ability, rank, aimPoint, target);
                return true;
            }
            Execute(ability, rank, target, aimPoint);
            return true;
        }

        public void CancelChannel()
        {
            ChannelSlot = -1;
            PendingSlot = -1;
        }

        public void CancelPending() => PendingSlot = -1;

        public void ReduceCooldown(CasterSlot slot, float remainingFraction)
        {
            NetSlot data = Slots[(int)slot];
            int remaining = data.ReadyTick - Runner.Tick;
            if (remaining <= 0) return;
            data.ReadyTick = Runner.Tick + Mathf.CeilToInt(remaining * Mathf.Clamp01(remainingFraction));
            Slots.Set((int)slot, data);
        }

        public void SwapSet(int setIndex, float durationSeconds, bool resetCooldowns)
        {
            if (!(_unit.Definition is ChampionDefinition champion)) return;
            AbilitySet set = setIndex <= 0 ? champion.DefaultAbilities : setIndex - 1 < champion.AlternateAbilitySets.Count ? champion.AlternateAbilitySets[setIndex - 1] : null;
            if (set == null) return;
            ActiveSet = (byte)Mathf.Max(0, setIndex);
            int duration = TickTime.SecondsToTicks(durationSeconds, Runner.TickRate);
            SetEndTick = duration > 0 && setIndex > 0 ? Runner.Tick + duration : 0;
            ApplySet(set, keepRanks: true);
            if (!resetCooldowns) return;
            for (int i = (int)CasterSlot.A; i <= (int)CasterSlot.R; i++)
            {
                NetSlot data = Slots[i];
                data.ReadyTick = 0;
                Slots.Set(i, data);
            }
        }

        public void StepSet(int step, bool resetCooldowns)
        {
            if (!(_unit.Definition is ChampionDefinition champion)) return;
            int count = 1 + champion.AlternateAbilitySets.Count;
            SwapSet(((ActiveSet + step) % count + count) % count, 0f, resetCooldowns);
        }

        public void Simulate()
        {
            int tick = Runner.Tick;
            if (SetEndTick > 0 && tick >= SetEndTick && ActiveSet > 0) SwapSet(0, 0f, false);
            RechargeCharges(tick);
            PayToggles();
            if (HasStateAuthority && TrinketOverrideEndTick > 0 && tick >= TrinketOverrideEndTick) RestoreTrinket();
            SyncTrinketRank();
            if (_unit.IsAlive)
            {
                SimulateChannel(tick);
                SimulatePending();
                SimulatePassive(tick);
            }
            Projectiles.Simulate();
            Areas.Simulate();
        }

        private bool Toggle(CasterSlot slot, AbilityDefinition ability, int rank, float cost)
        {
            StatusEffectDefinition status = ability.ToggleStatus;
            if (status == null) return false;
            if (_unit.Status.StacksOf(status) > 0)
            {
                _unit.Status.Remove(status);
                return true;
            }
            Commit(slot, ability, rank, cost);
            _unit.Status.Apply(new Combat.StatusApplication(status, _unit, rank, 1, 0f, null));
            Execute(ability, rank, null, _unit.Position);
            return true;
        }

        // Toggles pay their cost per second while on and switch off when it can no longer be paid.
        private void PayToggles()
        {
            if (!HasStateAuthority || _unit.Resource == null) return;
            for (int i = (int)CasterSlot.A; i <= (int)CasterSlot.R; i++)
            {
                AbilityDefinition ability = AbilityAt((CasterSlot)i);
                if (ability == null || ability.CastMode != CastMode.Toggle || ability.ToggleStatus == null || _unit.Status.StacksOf(ability.ToggleStatus) == 0) continue;
                float cost = ability.CostPerSecondAtRank(Mathf.Max(1, RankOf((CasterSlot)i))) * Runner.DeltaTime;
                if (_unit.Resource.CanAfford(cost)) _unit.Resource.Spend(cost);
                else _unit.Status.Remove(ability.ToggleStatus);
            }
        }

        private void Execute(AbilityDefinition ability, int rank, Unit target, Vector3 aimPoint)
        {
            LastCastTick = Runner.Tick;
            EffectExecutor.Run(ability.OnCast, new EffectContext(_unit, target, aimPoint, _unit.Position, rank, null));
            if (!string.IsNullOrEmpty(ability.BehaviourId)) AbilityBehaviours.OnExecuted(ability.BehaviourId, _unit, target, aimPoint);
        }

        private void Commit(CasterSlot slot, AbilityDefinition ability, int rank, float cost)
        {
            _unit.Resource?.Spend(cost);
            NetSlot data = Slots[(int)slot];
            int cooldownTicks = CooldownCalculator.ToTicks(ability.CooldownAtRank(rank), _unit.Stats.Get(StatType.AbilityHaste), _unit.Services.Rules.AbilityHasteScale, Runner.TickRate);
            if (ability.ChargesAt(rank) > 1)
            {
                if (data.Charges == ability.ChargesAt(rank)) data.ChargeTick = Runner.Tick + cooldownTicks;
                data.Charges = (byte)Mathf.Max(0, data.Charges - 1);
            }
            else data.ReadyTick = Runner.Tick + cooldownTicks;
            Slots.Set((int)slot, data);
            LastCastSlot = (sbyte)slot;
            PendingSlot = -1;
        }

        private void StartChannel(CasterSlot slot, AbilityDefinition ability, int rank, Vector3 point, Unit target)
        {
            _unit.Motor?.Stop();
            _unit.Attack?.ClearTarget();
            ChannelSlot = (sbyte)slot;
            ChannelStartTick = Runner.Tick;
            float seconds = ability.ChannelSecondsAt(rank);
            if (ability.BehaviourId == AbilityBehaviours.Recall) seconds *= Mathf.Clamp01(1f - _unit.Stats.Get(StatType.RecallTimeReduction));
            ChannelEndTick = Runner.Tick + TickTime.SecondsToTicks(seconds, Runner.TickRate);
            ChannelPoint = point;
            ChannelTarget = target != null ? target.Object.Id : default;
        }

        private void SimulateChannel(int tick)
        {
            if (!IsChanneling) return;
            CasterSlot slot = (CasterSlot)ChannelSlot;
            AbilityDefinition ability = AbilityAt(slot);
            bool interrupted = ability == null || !_unit.Status.CanCast || _unit.Health.LastDamagedTick > ChannelStartTick && AbilityBehaviours.IsInterruptedByDamage(ability.BehaviourId);
            if (interrupted)
            {
                CancelChannel();
                return;
            }
            if (tick < ChannelEndTick) return;
            ChannelSlot = -1;
            _unit.TryResolve(ChannelTarget, out Unit target);
            Execute(ability, Mathf.Max(1, RankOf(slot)), target, ChannelPoint);
        }

        private void SimulatePending()
        {
            if (PendingSlot < 0) return;
            if (!_unit.TryResolve(PendingTarget, out Unit target) || !target.IsAlive)
            {
                PendingSlot = -1;
                return;
            }
            CasterSlot slot = (CasterSlot)PendingSlot;
            AbilityDefinition ability = AbilityAt(slot);
            if (ability == null)
            {
                PendingSlot = -1;
                return;
            }
            if (InRange(target, ability.Targeting.RangeAt(Mathf.Max(1, RankOf(slot)))))
            {
                _unit.Motor?.Stop();
                TryCast(slot, target.Position, target);
            }
            else if (_unit.Motor != null && !_unit.Motor.IsMoving) _unit.Motor.MoveTo(target.Position, ability.Targeting.RangeAt(Mathf.Max(1, RankOf(slot))));
        }

        private void SimulatePassive(int tick)
        {
            if (!HasStateAuthority) return;
            AbilityDefinition passive = AbilityAt(CasterSlot.Passive);
            SyncReadyStatus(passive, tick);
            if (passive == null || string.IsNullOrEmpty(passive.BehaviourId)) return;
            NetSlot data = Slots[(int)CasterSlot.Passive];
            if (tick < data.ReadyTick || !AbilityBehaviours.CanTriggerPassive(passive.BehaviourId, _unit)) return;
            Unit target = passive.BehaviourId == AbilityBehaviours.PeriodicNearestAlly ? NearestAllyChampion(passive.Targeting.RangeAt(1)) : _unit;
            if (target == null) return;
            data.ReadyTick = tick + Mathf.Max(1, TickTime.SecondsToTicks(passive.CooldownAtRank(1), Runner.TickRate));
            Slots.Set((int)CasterSlot.Passive, data);
            EffectExecutor.Run(passive.OnCast, new EffectContext(_unit, target, target.Position, target.Position, 1, null));
        }

        // A passive can show it is armed with a status (Luffy's stretched punch), ranked by champion level.
        private void SyncReadyStatus(AbilityDefinition passive, int tick)
        {
            StatusEffectDefinition ready = passive != null ? passive.ReadyStatus : null;
            if (ready == null) return;
            bool armed = passive.BehaviourId == AbilityBehaviours.WhileInBrush ? IsInBrush() : tick >= Slots[(int)CasterSlot.Passive].ReadyTick;
            int stacks = _unit.Status.StacksOf(ready);
            int wanted = armed ? Mathf.Min(ready.MaxStacks, passive.ReadyStatusBaseStacks + _unit.Stats.Level) : 0;
            if (stacks == wanted) return;
            if (stacks > 0) _unit.Status.Remove(ready);
            if (wanted > 0) _unit.Status.Apply(new Combat.StatusApplication(ready, _unit, 1, wanted, 0f, null));
        }

        private bool IsInBrush()
        {
            Match.MapLayout map = _unit.Services.Match != null ? _unit.Services.Match.Map : null;
            return map != null && map.BrushAt(_unit.Position) >= 0;
        }

        private Unit NearestAllyChampion(float range)
        {
            _unit.Services.Units.Query(_unit.Position, range, _unit, TeamFilter.Allies, _allies);
            Unit best = null;
            float bestDistance = float.MaxValue;
            foreach (Unit ally in _allies)
            {
                if (!ally.IsChampion || !ally.IsAlive) continue;
                float distance = UnitRegistry.FlatDistance(_unit.Position, ally.Position);
                if (distance < bestDistance) { best = ally; bestDistance = distance; }
            }
            return best;
        }

        public void RaisePassiveTrigger(ItemTrigger trigger, Unit other)
        {
            AbilityDefinition passive = AbilityAt(CasterSlot.Passive);
            if (passive == null || !AbilityBehaviours.MatchesTrigger(passive.BehaviourId, trigger, other)) return;
            NetSlot data = Slots[(int)CasterSlot.Passive];
            int tick = Runner.Tick;
            if (tick < data.ReadyTick) return;
            int cooldown = TickTime.SecondsToTicks(passive.CooldownAtRank(1), Runner.TickRate);
            if (cooldown > 0)
            {
                data.ReadyTick = tick + cooldown;
                Slots.Set((int)CasterSlot.Passive, data);
            }
            Combat.CombatTriggers.Run(passive.OnCast, _unit, other, 1);
        }

        public bool HasTrinketOverride => TrinketOverrideEndTick > 0;

        public void ChooseTrinket(AbilityDefinition trinket)
        {
            if (!HasStateAuthority || trinket == null || HasTrinketOverride) return;
            int ready = Slots[(int)CasterSlot.Trinket].ReadyTick;
            SetSlotAbility(CasterSlot.Trinket, trinket, 1);
            BaseTrinket = Slots[(int)CasterSlot.Trinket].Ability;
            SetReadyTick(CasterSlot.Trinket, ready);
        }

        public void SetTrinketOverride(AbilityDefinition ability, float seconds)
        {
            if (!HasStateAuthority || ability == null) return;
            if (!HasTrinketOverride) TrinketSavedReadyTick = Slots[(int)CasterSlot.Trinket].ReadyTick;
            SetSlotAbility(CasterSlot.Trinket, ability, 1);
            SetReadyTick(CasterSlot.Trinket, 0);
            TrinketOverrideEndTick = Runner.Tick + Mathf.Max(1, TickTime.SecondsToTicks(seconds, Runner.TickRate));
        }

        public void RestoreTrinket()
        {
            if (!HasStateAuthority || !HasTrinketOverride) return;
            TrinketOverrideEndTick = 0;
            var abilities = _unit.Services.Content.Abilities;
            SetSlotAbility(CasterSlot.Trinket, BaseTrinket >= 0 && BaseTrinket < abilities.Count ? abilities[BaseTrinket] : null, 1);
            SetReadyTick(CasterSlot.Trinket, TrinketSavedReadyTick);
        }

        private void SetReadyTick(CasterSlot slot, int readyTick)
        {
            NetSlot data = Slots[(int)slot];
            data.ReadyTick = readyTick;
            Slots.Set((int)slot, data);
        }

        // The trinket has one rank per champion level, so its cooldown shrinks as the champion levels up.
        private void SyncTrinketRank()
        {
            if (!HasStateAuthority) return;
            AbilityDefinition trinket = AbilityAt(CasterSlot.Trinket);
            if (trinket == null) return;
            NetSlot data = Slots[(int)CasterSlot.Trinket];
            byte rank = (byte)Mathf.Clamp(_unit.Stats.Level, 1, trinket.MaxRank);
            if (data.Rank == rank) return;
            data.Rank = rank;
            Slots.Set((int)CasterSlot.Trinket, data);
        }

        private void RechargeCharges(int tick)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                NetSlot data = Slots[i];
                AbilityDefinition ability = AbilityAt((CasterSlot)i);
                if (ability == null || data.Rank == 0) continue;
                int maxCharges = ability.ChargesAt(data.Rank);
                if (maxCharges <= 1 || data.Charges >= maxCharges || tick < data.ChargeTick) continue;
                data.Charges++;
                int cooldownTicks = CooldownCalculator.ToTicks(ability.CooldownAtRank(data.Rank), _unit.Stats.Get(StatType.AbilityHaste), _unit.Services.Rules.AbilityHasteScale, Runner.TickRate);
                data.ChargeTick = data.Charges < maxCharges ? tick + cooldownTicks : 0;
                Slots.Set(i, data);
            }
        }

        private Unit ResolveUnitTarget(TargetingData targeting, Unit aimTarget, float range)
        {
            if (aimTarget != null && aimTarget.IsAlive && aimTarget.IsTargetable && TeamRules.Matches(targeting.ValidTargets, _unit, aimTarget)) return aimTarget;
            if (targeting.Fallback != TargetFallback.LowestHealthAllyInRange) return null;
            List<Unit> allies = new List<Unit>();
            _unit.Services.Units.Query(_unit.Position, range, _unit, TeamFilter.Self | TeamFilter.Allies, allies);
            Unit best = null;
            foreach (Unit ally in allies)
                if (ally.IsChampion && (best == null || ally.Health.Ratio < best.Health.Ratio)) best = ally;
            return best;
        }

        private bool InRange(Unit target, float range) =>
            UnitRegistry.FlatDistance(_unit.Position, target.Position) - target.Radius - _unit.Radius <= range;

        private void ApplySet(AbilitySet set, bool keepRanks)
        {
            for (AbilitySlot slot = AbilitySlot.A; slot <= AbilitySlot.R; slot++)
            {
                CasterSlot casterSlot = (CasterSlot)(int)slot;
                NetSlot data = Slots[(int)casterSlot];
                StopLeavingToggle(AbilityAt(casterSlot), set.Get(slot));
                SetSlotAbility(casterSlot, set.Get(slot), keepRanks ? data.Rank : 0);
            }
        }

        private void StopLeavingToggle(AbilityDefinition leaving, AbilityDefinition coming)
        {
            StatusEffectDefinition status = leaving != null && leaving.CastMode == CastMode.Toggle ? leaving.ToggleStatus : null;
            if (status == null || coming != null && coming.ToggleStatus == status) return;
            _unit.Status.Remove(status);
        }

        private void SetSlotAbility(CasterSlot slot, AbilityDefinition ability, int rank)
        {
            NetSlot data = Slots[(int)slot];
            data.Ability = ability != null ? (short)_unit.Services.Content.Abilities.IndexOf(ability.Id) : (short)NoAbility;
            data.Rank = (byte)(ability != null ? Mathf.Min(rank, ability.MaxRank) : 0);
            data.Charges = (byte)(ability != null && data.Rank > 0 ? ability.ChargesAt(data.Rank) : 0);
            Slots.Set((int)slot, data);
        }

        private static bool HasDash(AbilityDefinition ability)
        {
            foreach (var effect in ability.OnCast)
                if (effect is Data.Effects.DashEffect) return true;
            return false;
        }
    }
}
