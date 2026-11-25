using System;
using Fusion;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Simulation;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Combat
{
    public struct NetShield : INetworkStruct
    {
        public float Remaining;
        public byte Filter;
        public int ExpiresTick;
        public NetworkBool BlocksCrowdControl;
    }

    public struct HitRecord : INetworkStruct
    {
        public NetworkId Source;
        public int Tick;
        public float Amount;
        public byte Type;
        public byte Flags;
        public NetworkBool IsHeal;
    }

    public readonly struct DamageInfo
    {
        public DamageInfo(Unit source, float amount, DamageType type, DamageFlags flags)
        {
            Source = source;
            Amount = amount;
            Type = type;
            Flags = flags;
        }

        public Unit Source { get; }
        public float Amount { get; }
        public DamageType Type { get; }
        public DamageFlags Flags { get; }
    }

    public sealed class Health : NetworkBehaviour
    {
        public const int HitHistory = 8;
        private const int ShieldCapacity = 4;

        private readonly ShieldPool _pool = new ShieldPool();
        private Unit _unit;

        [Networked] public float Current { get; set; }
        [Networked] public NetworkBool IsDead { get; set; }
        [Networked] public int DeathTick { get; set; }
        [Networked] public int LastDamagedTick { get; set; }
        [Networked] public int LastCombatTick { get; set; }
        [Networked] public NetworkId LastAttacker { get; set; }
        [Networked] public int LastHarassTick { get; set; }
        [Networked] public int HitCount { get; set; }
        [Networked, Capacity(ShieldCapacity)] public NetworkArray<NetShield> Shields => default;
        [Networked, Capacity(HitHistory)] public NetworkArray<HitRecord> Hits => default;

        public event Action<Unit, Unit> Died;

        public float Max => _unit != null ? Mathf.Max(1f, _unit.Stats.Get(StatType.MaxHealth)) : 1f;
        public float Ratio => Mathf.Clamp01(Current / Max);

        public float ShieldTotal
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < Shields.Length; i++) total += Shields[i].Remaining;
                return total;
            }
        }

        public bool BlocksCrowdControl
        {
            get
            {
                for (int i = 0; i < Shields.Length; i++)
                    if (Shields[i].Remaining > 0f && Shields[i].BlocksCrowdControl) return true;
                return false;
            }
        }

        public void Initialize(Unit unit)
        {
            _unit = unit;
            if (!HasStateAuthority) return;
            _unit.Stats.Refresh();
            Current = Max;
            IsDead = false;
        }

        public void Simulate()
        {
            if (!HasStateAuthority) return;
            int tick = Runner.Tick;
            for (int i = 0; i < Shields.Length; i++)
            {
                NetShield shield = Shields[i];
                if (shield.Remaining > 0f && shield.ExpiresTick > 0 && shield.ExpiresTick <= tick) Shields.Set(i, default);
            }
            if (IsDead) return;
            float regen = _unit.Stats.Get(StatType.HealthRegen);
            if (regen > 0f && Current < Max) Current = Mathf.Min(Max, Current + regen * Runner.DeltaTime);
            if (Current > Max) Current = Max;
        }

        public DamageResult ApplyDamage(in DamageInfo info)
        {
            if (!HasStateAuthority || IsDead || _unit.IsInvulnerable || info.Amount <= 0f) return DamageResult.None;

            float amount = info.Amount;
            if (info.Source != null && info.Source.Kind == Data.Definitions.UnitKind.Minion && (_unit.IsChampion || _unit.IsStructure))
                amount *= _unit.Services.Rules.MinionDamageToChampionsRatio;

            LoadShields();
            DamageRequest request = new DamageRequest(amount, info.Type, info.Flags);
            DamageResult result = DamageCalculator.Resolve(request, info.Source != null ? info.Source.Stats : null, _unit.Stats, _pool, _unit.Services.Rules.CombatRules);
            StoreShields();

            int tick = Runner.Tick;
            Current = Mathf.Max(0f, Current - result.HealthDamage);
            LastDamagedTick = tick;
            LastCombatTick = tick;
            if (info.Source != null)
            {
                LastAttacker = info.Source.Object.Id;
                info.Source.Health.LastCombatTick = tick;
                if (info.Source.IsChampion && _unit.IsChampion) info.Source.Health.LastHarassTick = tick;
            }
            Record(info.Source, result.AfterMitigation, info.Type, info.Flags, false);

            if (Current <= 0f) Kill(info.Source);
            else if (result.AfterMitigation > 0f && (info.Flags & DamageFlags.Proc) == 0) CombatTriggers.Raise(_unit, ItemTrigger.OnDamageTaken, info.Source);
            return result;
        }

        public float Heal(float amount, Unit source)
        {
            if (!HasStateAuthority || IsDead || amount <= 0f) return 0f;
            float scaled = amount * Mathf.Max(0f, 1f + _unit.Stats.Get(StatType.HealPower));
            float applied = Mathf.Min(scaled, Max - Current);
            if (applied <= 0f) return 0f;
            Current += applied;
            Record(source, applied, DamageType.True, DamageFlags.None, true);
            CombatTriggers.Raise(source, ItemTrigger.OnHeal, _unit);
            return applied;
        }

        public void AddShield(float amount, ShieldFilter filter, float durationSeconds, bool blocksCrowdControl)
        {
            if (!HasStateAuthority || IsDead || amount <= 0f) return;
            int slot = 0;
            float weakest = float.MaxValue;
            for (int i = 0; i < Shields.Length; i++)
            {
                float remaining = Shields[i].Remaining;
                if (remaining < weakest)
                {
                    weakest = remaining;
                    slot = i;
                }
            }
            int duration = TickTime.SecondsToTicks(durationSeconds, Runner.TickRate);
            Shields.Set(slot, new NetShield
            {
                Remaining = amount,
                Filter = (byte)filter,
                ExpiresTick = duration > 0 ? Runner.Tick + duration : 0,
                BlocksCrowdControl = blocksCrowdControl
            });
        }

        public void Revive(float ratio = 1f)
        {
            if (!HasStateAuthority) return;
            IsDead = false;
            _unit.Stats.Refresh();
            Current = Max * Mathf.Clamp01(ratio);
            for (int i = 0; i < Shields.Length; i++) Shields.Set(i, default);
        }

        public void SetCurrent(float value)
        {
            if (HasStateAuthority) Current = Mathf.Clamp(value, 0f, Max);
        }

        public bool IsInCombat(float windowSeconds) =>
            LastCombatTick > 0 && Runner.Tick - LastCombatTick < TickTime.SecondsToTicks(windowSeconds, Runner.TickRate);

        public HitRecord HitAt(int sequence) => Hits[((sequence % HitHistory) + HitHistory) % HitHistory];

        private void Kill(Unit killer)
        {
            IsDead = true;
            DeathTick = Runner.Tick;
            _unit.Status.ClearAll();
            Died?.Invoke(_unit, killer);
            _unit.Services.Match?.OnUnitKilled(_unit, killer);
            CombatTriggers.Raise(killer, ItemTrigger.OnKill, _unit);
        }

        private void Record(Unit source, float amount, DamageType type, DamageFlags flags, bool isHeal)
        {
            Hits.Set(HitCount % HitHistory, new HitRecord
            {
                Source = source != null ? source.Object.Id : default,
                Tick = Runner.Tick,
                Amount = amount,
                Type = (byte)type,
                Flags = (byte)flags,
                IsHeal = isHeal
            });
            HitCount++;
        }

        private void LoadShields()
        {
            while (_pool.Shields.Count > 0) _pool.Remove(_pool.Shields[0].Id);
            for (int i = 0; i < Shields.Length; i++)
            {
                NetShield shield = Shields[i];
                if (shield.Remaining > 0f) _pool.Add(new Shield(i, shield.Remaining, (ShieldFilter)shield.Filter, shield.ExpiresTick, shield.BlocksCrowdControl));
            }
        }

        private void StoreShields()
        {
            for (int i = 0; i < Shields.Length; i++)
            {
                NetShield stored = Shields[i];
                if (stored.Remaining <= 0f) continue;
                stored.Remaining = 0f;
                foreach (Shield shield in _pool.Shields)
                    if (shield.Id == i) stored.Remaining = shield.Remaining;
                if (stored.Remaining <= 0f) stored = default;
                Shields.Set(i, stored);
            }
        }
    }
}
