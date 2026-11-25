using System.Collections.Generic;
using Fusion;
using MangaLegends.Core.Simulation;
using MangaLegends.Core.Stats;
using MangaLegends.Core.Status;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Serialization;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Units;

namespace MangaLegends.Simulation.Combat
{
    public struct NetStatus : INetworkStruct
    {
        public short Definition;
        public int ExpiresTick;
        public int NextPulseTick;
        public byte Stacks;
        public byte Rank;
        public NetworkId Source;
        public byte OverrideCount;
        public int TriggerReadyTick;
        public byte Stat0, Kind0, Stat1, Kind1, Stat2, Kind2;
        public float Value0, Value1, Value2;

        public bool IsActive => Stacks > 0;
    }

    public readonly struct StatusApplication
    {
        public StatusApplication(StatusEffectDefinition definition, Unit source, int rank, int stacks, float durationSeconds, IReadOnlyList<RankedStatModifierData> overrides)
        {
            Definition = definition;
            Source = source;
            Rank = rank;
            Stacks = stacks;
            DurationSeconds = durationSeconds;
            Overrides = overrides;
        }

        public StatusEffectDefinition Definition { get; }
        public Unit Source { get; }
        public int Rank { get; }
        public int Stacks { get; }
        public float DurationSeconds { get; }
        public IReadOnlyList<RankedStatModifierData> Overrides { get; }
    }

    public sealed class StatusEffects : NetworkBehaviour, IStatModifierSource
    {
        private const int SourceBase = 1000;
        private const int Capacity = 12;

        private Unit _unit;

        [Networked, Capacity(Capacity)] public NetworkArray<NetStatus> Active => default;
        [Networked] public int Version { get; set; }

        public int ModifierVersion => Version;
        public bool CanMove => !IsRestricted(StatusRestriction.Move);
        public bool CanCast => !IsRestricted(StatusRestriction.Cast);
        public bool CanAttack => !IsRestricted(StatusRestriction.Attack);
        public bool IsTargetable => !IsRestricted(StatusRestriction.BeTargeted);

        public void Initialize(Unit unit) => _unit = unit;

        public void Simulate()
        {
            int tick = Runner.Tick;
            for (int i = 0; i < Active.Length; i++)
            {
                NetStatus status = Active[i];
                if (!status.IsActive) continue;
                if (status.ExpiresTick > 0 && status.ExpiresTick <= tick)
                {
                    Clear(i);
                    continue;
                }
                if (HasStateAuthority && status.NextPulseTick > 0 && status.NextPulseTick <= tick) Pulse(i, status, tick);
            }
        }

        public StatusApplyOutcome Apply(in StatusApplication application)
        {
            StatusEffectDefinition definition = application.Definition;
            if (!HasStateAuthority || definition == null || _unit == null || (!_unit.IsAlive && !definition.PersistsThroughDeath)) return StatusApplyOutcome.Ignored;
            if (definition.IsCrowdControl && _unit.Health.BlocksCrowdControl) return StatusApplyOutcome.BlockedByImmunity;

            short index = (short)_unit.Services.Content.StatusEffects.IndexOf(definition.Id);
            if (index < 0) return StatusApplyOutcome.Ignored;
            int tick = Runner.Tick;
            int durationTicks = TickTime.SecondsToTicks(application.DurationSeconds > 0f ? application.DurationSeconds : definition.DurationSeconds, Runner.TickRate);
            int expires = durationTicks > 0 ? tick + durationTicks : 0;

            int slot = definition.Stacking == StackingRule.Independent ? -1 : Find(index);
            if (slot >= 0)
            {
                NetStatus existing = Active[slot];
                if (definition.Stacking == StackingRule.KeepExisting) return StatusApplyOutcome.Ignored;
                bool stacked = definition.Stacking == StackingRule.AddStack && existing.Stacks < definition.MaxStacks;
                if (stacked) existing.Stacks = (byte)System.Math.Min(definition.MaxStacks, existing.Stacks + application.Stacks);
                existing.ExpiresTick = expires;
                existing.Rank = (byte)application.Rank;
                existing.Source = application.Source != null ? application.Source.Object.Id : default;
                WriteOverrides(ref existing, application);
                Active.Set(slot, existing);
                Version++;
                CheckMaxStacks(slot, definition, application.Source);
                return stacked ? StatusApplyOutcome.Stacked : StatusApplyOutcome.Refreshed;
            }

            slot = FreeSlot();
            if (slot < 0) return StatusApplyOutcome.Ignored;
            NetStatus created = new NetStatus
            {
                Definition = index,
                ExpiresTick = expires,
                Stacks = (byte)System.Math.Max(1, System.Math.Min(definition.MaxStacks, application.Stacks)),
                Rank = (byte)application.Rank,
                Source = application.Source != null ? application.Source.Object.Id : default,
                NextPulseTick = definition.TickIntervalSeconds > 0f ? tick + TickTime.SecondsToTicks(definition.TickIntervalSeconds, Runner.TickRate) : 0
            };
            WriteOverrides(ref created, application);
            Active.Set(slot, created);
            Version++;
            CheckMaxStacks(slot, definition, application.Source);
            return StatusApplyOutcome.Added;
        }

        public int RemoveCrowdControl()
        {
            if (!HasStateAuthority) return 0;
            int removed = 0;
            for (int i = 0; i < Active.Length; i++)
            {
                StatusEffectDefinition definition = DefinitionAt(i);
                if (definition == null || !definition.IsCrowdControl) continue;
                Clear(i);
                removed++;
            }
            return removed;
        }

        public void Remove(StatusEffectDefinition definition)
        {
            if (!HasStateAuthority || definition == null || _unit == null) return;
            int index = _unit.Services.Content.StatusEffects.IndexOf(definition.Id);
            for (int i = 0; i < Active.Length; i++)
                if (Active[i].IsActive && Active[i].Definition == index) Clear(i);
        }

        public void ClearAll()
        {
            for (int i = 0; i < Active.Length; i++)
            {
                if (!Active[i].IsActive) continue;
                StatusEffectDefinition definition = DefinitionAt(i);
                if (definition == null || !definition.PersistsThroughDeath) Clear(i);
            }
        }

        public bool TryStartTriggerCooldown(int slot, float cooldownSeconds)
        {
            NetStatus status = Active[slot];
            if (Runner.Tick < status.TriggerReadyTick) return false;
            if (cooldownSeconds <= 0f) return true;
            status.TriggerReadyTick = Runner.Tick + TickTime.SecondsToTicks(cooldownSeconds, Runner.TickRate);
            Active.Set(slot, status);
            return true;
        }

        private void CheckMaxStacks(int slot, StatusEffectDefinition definition, Unit source)
        {
            if (definition.OnMaxStacks.Count == 0 || Active[slot].Stacks < definition.MaxStacks) return;
            int rank = Active[slot].Rank;
            Clear(slot);
            EffectExecutor.Run(definition.OnMaxStacks, EffectContext.ForTarget(source != null ? source : _unit, _unit, rank).AsProc());
        }

        public int StacksOf(StatusEffectDefinition definition)
        {
            if (definition == null || _unit == null) return 0;
            int index = _unit.Services.Content.StatusEffects.IndexOf(definition.Id);
            int total = 0;
            for (int i = 0; i < Active.Length; i++)
                if (Active[i].IsActive && Active[i].Definition == index) total += Active[i].Stacks;
            return total;
        }

        public StatusEffectDefinition DefinitionAt(int slot)
        {
            NetStatus status = Active[slot];
            if (!status.IsActive || _unit == null) return null;
            var statuses = _unit.Services.Content.StatusEffects;
            return status.Definition >= 0 && status.Definition < statuses.Count ? statuses[status.Definition] : null;
        }

        public void CollectModifiers(List<StatModifier> into)
        {
            for (int i = 0; i < Active.Length; i++)
            {
                NetStatus status = Active[i];
                StatusEffectDefinition definition = DefinitionAt(i);
                if (definition == null) continue;
                int source = SourceBase + i;
                if (status.OverrideCount > 0)
                {
                    AddOverride(into, status.Stat0, status.Kind0, status.Value0, status.Stacks, source, status.OverrideCount > 0);
                    AddOverride(into, status.Stat1, status.Kind1, status.Value1, status.Stacks, source, status.OverrideCount > 1);
                    AddOverride(into, status.Stat2, status.Kind2, status.Value2, status.Stacks, source, status.OverrideCount > 2);
                    continue;
                }
                foreach (StatModifierData modifier in definition.ModifiersPerStack) into.Add(modifier.ToModifier(source, status.Stacks));
            }
        }

        public bool IsInvulnerable
        {
            get
            {
                for (int i = 0; i < Active.Length; i++)
                {
                    StatusEffectDefinition definition = DefinitionAt(i);
                    if (definition != null && definition.Invulnerable) return true;
                }
                return false;
            }
        }

        public float AbilityDamageReduction
        {
            get
            {
                float best = 0f;
                for (int i = 0; i < Active.Length; i++)
                {
                    StatusEffectDefinition definition = DefinitionAt(i);
                    if (definition != null && definition.AbilityDamageReduction > best) best = definition.AbilityDamageReduction;
                }
                return best;
            }
        }

        public bool TryGetTaunt(out NetworkId source) => TryFind(d => d.Taunt, out source, out _);

        public bool TryGetRedirect(out NetworkId source, out float ratio)
        {
            ratio = 0f;
            if (!TryFind(d => d.RedirectToSource > 0f, out source, out int slot)) return false;
            ratio = DefinitionAt(slot).RedirectToSource;
            return true;
        }

        public bool TryConsumeAttackBlock()
        {
            if (!HasStateAuthority || !TryFind(d => d.BlocksNextBasicAttack, out _, out int slot)) return false;
            Clear(slot);
            return true;
        }

        public void RemoveAt(int slot)
        {
            if (HasStateAuthority && Active[slot].IsActive) Clear(slot);
        }

        private bool TryFind(System.Predicate<StatusEffectDefinition> match, out NetworkId source, out int slot)
        {
            for (int i = 0; i < Active.Length; i++)
            {
                StatusEffectDefinition definition = DefinitionAt(i);
                if (definition == null || !match(definition)) continue;
                source = Active[i].Source;
                slot = i;
                return true;
            }
            source = default;
            slot = -1;
            return false;
        }

        private bool IsRestricted(StatusRestriction restriction)
        {
            for (int i = 0; i < Active.Length; i++)
            {
                StatusEffectDefinition definition = DefinitionAt(i);
                if (definition != null && (definition.Restrictions & restriction) != 0) return true;
            }
            return false;
        }

        private void Pulse(int slot, NetStatus status, int tick)
        {
            StatusEffectDefinition definition = DefinitionAt(slot);
            if (definition == null) return;
            status.NextPulseTick = tick + System.Math.Max(1, TickTime.SecondsToTicks(definition.TickIntervalSeconds, Runner.TickRate));
            Active.Set(slot, status);
            _unit.TryResolve(status.Source, out Unit source);
            EffectExecutor.Run(definition.OnTick, EffectContext.ForTarget(source != null ? source : _unit, _unit, status.Rank).AsProc());
        }

        private int Find(short definition)
        {
            for (int i = 0; i < Active.Length; i++)
                if (Active[i].IsActive && Active[i].Definition == definition) return i;
            return -1;
        }

        private int FreeSlot()
        {
            for (int i = 0; i < Active.Length; i++)
                if (!Active[i].IsActive) return i;
            return -1;
        }

        private void Clear(int slot)
        {
            Active.Set(slot, default);
            Version++;
        }

        private static void WriteOverrides(ref NetStatus status, in StatusApplication application)
        {
            status.OverrideCount = 0;
            if (application.Overrides == null) return;
            for (int i = 0; i < application.Overrides.Count && i < 3; i++)
            {
                RankedStatModifierData modifier = application.Overrides[i];
                float value = modifier.Value.At(application.Rank);
                if (i == 0) { status.Stat0 = (byte)modifier.Stat; status.Kind0 = (byte)modifier.Kind; status.Value0 = value; }
                if (i == 1) { status.Stat1 = (byte)modifier.Stat; status.Kind1 = (byte)modifier.Kind; status.Value1 = value; }
                if (i == 2) { status.Stat2 = (byte)modifier.Stat; status.Kind2 = (byte)modifier.Kind; status.Value2 = value; }
                status.OverrideCount++;
            }
        }

        private static void AddOverride(List<StatModifier> into, byte stat, byte kind, float value, int stacks, int source, bool present)
        {
            if (!present) return;
            into.Add(new StatModifier((StatType)stat, (ModifierKind)kind, value * stacks, source));
        }
    }
}
