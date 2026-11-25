using System.Collections.Generic;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Scaling;
using MangaLegends.Core.Simulation;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Data.Effects;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Abilities
{
    // Effects that only move the caster or create predicted entities run on every simulating peer;
    // everything that changes health, statuses or other units runs on the state authority only.
    public static class EffectExecutor
    {
        public static void Run(IReadOnlyList<EffectDefinition> effects, in EffectContext context)
        {
            if (effects == null || context.Caster == null) return;
            foreach (EffectDefinition effect in effects)
                if (effect != null) Execute(effect, context);
        }

        private static void Execute(EffectDefinition effect, in EffectContext context)
        {
            Unit caster = context.Caster;
            bool authority = caster.Object != null && caster.Runner.IsServer;
            switch (effect)
            {
                case DamageEffect damage when authority:
                    foreach (Unit target in Targets(damage.Target, context)) Damage(damage, context, target);
                    break;
                case HealEffect heal when authority:
                    foreach (Unit target in Targets(heal.Target, context)) Heal(heal, context, target);
                    break;
                case ShieldEffect shield when authority:
                    foreach (Unit target in Targets(shield.Target, context))
                        target.Health.AddShield(shield.Amount.Evaluate(context.Rank, Scaling(caster, target, 0)), shield.Filter, shield.DurationSecondsAt(context.Rank), shield.BlocksCrowdControl);
                    break;
                case ApplyStatusEffect status when authority:
                    foreach (Unit target in Targets(status.Target, context))
                    {
                        if (status.RequiredStatus != null && !status.AcceptsStacks(target.Status.StacksOf(status.RequiredStatus))) continue;
                        if (status.OnePerCaster) RemoveFromOthers(status.Status, caster, target);
                        target.Status.Apply(new StatusApplication(status.Status, caster, context.Rank, status.StacksAt(context.Rank), status.DurationSecondsAt(context.Rank), status.ModifierOverrides, context.AimDirection));
                        target.Stats.Refresh();
                    }
                    break;
                case RemoveStatusEffect remove when authority:
                    foreach (Unit target in Targets(remove.Target, context)) target.Status.Remove(remove.Status);
                    break;
                case CleanseEffect cleanse when authority:
                    foreach (Unit target in Targets(cleanse.Target, context)) target.Status.RemoveCrowdControl();
                    break;
                case DisplaceEffect displace when authority:
                    foreach (Unit target in Targets(EffectTarget.Target, context)) Displace(displace, context, target);
                    break;
                case TeleportEffect teleport when authority:
                    Teleport(teleport, context);
                    break;
                case SwapPositionsEffect _ when authority:
                    SwapPositions(context);
                    break;
                case DashEffect dash:
                    Dash(dash, context);
                    break;
                case LaunchProjectileEffect launch:
                    Launch(launch, context);
                    break;
                case CreateAreaEffect area:
                    if (caster.Caster != null && area.Area != null)
                        caster.Caster.Areas.Spawn(area.Area, context.Rank, area.AtAimPoint ? context.AimPoint : context.ImpactPoint, context.AimDirection);
                    break;
                case SwapAbilitySetEffect swap:
                    if (swap.Step != 0) caster.Caster?.StepSet(swap.Step, swap.ResetCooldowns);
                    else caster.Caster?.SwapSet(swap.AlternateSetIndex + 1, swap.DurationSecondsAt(context.Rank), swap.ResetCooldowns);
                    break;
                case ReduceCooldownEffect reduce:
                    caster.Caster?.ReduceCooldown((CasterSlot)(int)reduce.Slot, reduce.RemainingFractionAt(context.Rank));
                    break;
                case PlaceWardEffect ward when authority:
                    caster.Services.Match?.PlaceWard(caster, ward.Ward, context.AimPoint, ward.LifetimeSeconds, ward.MaxPerOwner);
                    break;
                case ExecuteEffect execute when authority:
                    Execute(execute, context);
                    break;
                case SummonEffect summon when authority:
                    caster.Services.Match?.SpawnSummon(caster, summon.Unit, summon.CountAt(context.Rank), summon.DurationSecondsAt(context.Rank), context.AimPoint);
                    break;
                case DrainResourceEffect drain when authority:
                    Drain(drain, context);
                    break;
                case PayHealthEffect pay when authority:
                    caster.Health.SetCurrent(Mathf.Max(1f, caster.Health.Current - pay.Amount.Evaluate(context.Rank, Scaling(caster, caster, 0))));
                    break;
                case ResetAttackEffect _:
                    caster.Attack?.ResetTimer();
                    break;
            }
        }

        private static IReadOnlyList<Unit> Targets(EffectTarget target, in EffectContext context)
        {
            switch (target)
            {
                case EffectTarget.Caster:
                    return One(context.Caster);
                case EffectTarget.AllHitUnits:
                    return context.HitUnits ?? One(context.Target);
                default:
                    return One(context.Target);
            }
        }

        private static IReadOnlyList<Unit> One(Unit unit) =>
            unit != null && unit.IsAlive ? new[] { unit } : System.Array.Empty<Unit>();

        private static ScalingContext Scaling(Unit caster, Unit target, int stacks) =>
            new ScalingContext(caster.Stats, target != null ? target.Health.Max : 0f, target != null ? target.Health.Current : 0f, stacks);

        private static void Damage(DamageEffect damage, in EffectContext context, Unit target)
        {
            Unit caster = context.Caster;
            if (!caster.IsEnemyOf(target)) return;
            Unit stackHolder = damage.StacksOnCaster ? caster : target;
            int stacks = damage.StacksFrom != null ? stackHolder.Status.StacksOf(damage.StacksFrom) : 0;
            if (stacks < damage.MinStacks) return;
            if (damage.ConsumesStacks && damage.StacksFrom != null) stackHolder.Status.Remove(damage.StacksFrom);
            float amount = damage.Amount.Evaluate(context.Rank, Scaling(caster, target, stacks));
            DamageFlags flags = context.IsProc ? DamageFlags.Proc : DamageFlags.Ability;
            if (damage.CanCrit)
            {
                DeterministicRandom random = DeterministicRandom.ForTick(caster.Services.Match != null ? caster.Services.Match.Seed : 0UL, caster.Tick, (int)target.SortKey);
                if (random.Roll(caster.Stats.Get(StatType.CritChance)))
                {
                    amount *= Mathf.Max(1f, caster.Stats.Get(StatType.CritDamage));
                    flags |= DamageFlags.Critical;
                }
            }
            DamageResult result = target.Health.ApplyDamage(new DamageInfo(caster, amount, damage.Type, flags));
            if (context.IsProc || result.AfterMitigation <= 0f) return;
            CombatTriggers.Raise(caster, ItemTrigger.OnAbilityHit, target);
            if (damage.AppliesOnHitEffects) CombatTriggers.Raise(caster, ItemTrigger.OnHit, target);
        }

        private static void Execute(ExecuteEffect execute, in EffectContext context)
        {
            Unit target = context.Target;
            if (target == null || !target.IsAlive || !context.Caster.IsEnemyOf(target)) return;
            if (execute.ChampionsOnly && !target.IsChampion) return;
            if (target.Health.Ratio > execute.HealthRatio) return;
            float lethal = target.Health.Current + target.Health.ShieldTotal + 1f;
            target.Health.ApplyDamage(new DamageInfo(context.Caster, lethal, DamageType.True, DamageFlags.IgnoreShields | DamageFlags.Proc));
        }

        private static void Heal(HealEffect heal, in EffectContext context, Unit target)
        {
            if (context.Caster.IsEnemyOf(target) && !heal.AnyTeam) return;
            float amount = heal.Amount.Evaluate(context.Rank, Scaling(context.Caster, target, 0));
            if (heal.Pool == HealedPool.Resource) target.Resource?.Restore(amount);
            else target.Health.Heal(amount, context.Caster);
        }

        private static void Dash(DashEffect dash, in EffectContext context)
        {
            Unit caster = context.Caster;
            if (caster.Motor == null) return;
            float maxDistance = dash.MaxDistanceAt(context.Rank);
            Vector3 origin = caster.Position;
            Vector3 end;
            switch (dash.Destination)
            {
                case DashDestination.ToTarget when context.Target != null:
                    end = context.Target.Position - (context.Target.Position - origin).normalized * (context.Target.Radius + caster.Radius);
                    break;
                case DashDestination.BehindTarget when context.Target != null:
                    end = context.Target.Position + (context.Target.Position - origin).normalized * (context.Target.Radius + caster.Radius + 0.2f);
                    break;
                default:
                    Vector3 offset = context.AimPoint - origin;
                    offset.y = 0f;
                    end = origin + Vector3.ClampMagnitude(offset, maxDistance);
                    break;
            }
            float speed = dash.Destination == DashDestination.Blink ? 0f : dash.SpeedAt(context.Rank);
            caster.Motor.StartDash(end, speed, dash.PassesThroughWalls || dash.Destination == DashDestination.Blink);
        }

        private static void Displace(DisplaceEffect displace, in EffectContext context, Unit target)
        {
            if (target.Motor == null || !context.Caster.IsEnemyOf(target) || target.IsStructure) return;
            Vector3 origin = displace.FromImpactPoint ? context.ImpactPoint : context.Caster.Position;
            Vector3 away = target.Position - origin;
            away.y = 0f;
            away = away.sqrMagnitude > 0.0001f ? away.normalized : context.Caster.transform.forward;
            if (displace.Kind == DisplacementKind.Random) away = RandomDirection(context.Caster, target);
            float distance = displace.DistanceAt(context.Rank);
            float duration = Mathf.Max(0.05f, displace.DurationSecondsAt(context.Rank));
            Vector3 direction = displace.Kind == DisplacementKind.Pull ? -away : away;
            if (displace.Kind == DisplacementKind.Knockup) distance = 0f;
            if (displace.Kind == DisplacementKind.Pull && displace.FromImpactPoint) distance = Mathf.Min(distance, UnitRegistry.FlatDistance(target.Position, origin));
            target.Motor.StartDash(target.Position + direction * distance, distance / duration, false);
        }

        private static void RemoveFromOthers(StatusEffectDefinition status, Unit caster, Unit keep)
        {
            foreach (Unit unit in caster.Services.Units.All)
                if (unit != null && unit != keep) unit.Status.RemoveFromCaster(status, caster.Object.Id);
        }

        private static Vector3 RandomDirection(Unit caster, Unit target)
        {
            DeterministicRandom random = DeterministicRandom.ForTick(caster.Services.Match != null ? caster.Services.Match.Seed : 0UL, caster.Tick, (int)target.SortKey);
            float angle = random.NextFloat() * 360f;
            return Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        }

        private static void Drain(DrainResourceEffect drain, in EffectContext context)
        {
            Unit target = context.Target;
            if (target == null || target.Resource == null || !target.Resource.UsesResource || !context.Caster.IsEnemyOf(target)) return;
            float amount = target.Resource.Current * drain.CurrentRatioAt(context.Rank);
            if (amount <= 0f) return;
            target.Resource.Spend(amount);
            context.Caster.Resource?.Restore(amount);
        }

        private static void Teleport(TeleportEffect teleport, in EffectContext context)
        {
            Unit destination = context.Target;
            if (destination == null || destination.Team != context.Caster.Team || !destination.IsAlive) return;
            if (System.Array.IndexOf(teleport.AllowedDestinations, destination.Kind) < 0) return;
            Vector3 offset = context.Caster.Position - destination.Position;
            offset.y = 0f;
            offset = offset.sqrMagnitude > 0.0001f ? offset.normalized : Vector3.forward;
            context.Caster.Motor?.Teleport(destination.Position + offset * (destination.Radius + teleport.ArrivalOffset));
        }

        private static void SwapPositions(in EffectContext context)
        {
            Unit target = context.Target;
            if (target == null || target.Motor == null || context.Caster.Motor == null || target.IsStructure) return;
            Vector3 casterPosition = context.Caster.Position;
            context.Caster.Motor.Teleport(target.Position);
            target.Motor.Teleport(casterPosition);
        }

        private static void Launch(LaunchProjectileEffect launch, in EffectContext context)
        {
            Unit caster = context.Caster;
            if (caster.Caster == null || launch.Projectile == null) return;
            int count = launch.CountAt(context.Rank);
            float spread = launch.SpreadAngleAt(context.Rank);
            Vector3 direction = context.Target != null ? Flat(context.Target.Position - caster.Position) : context.AimDirection;
            for (int i = 0; i < count; i++)
            {
                float angle = count > 1 ? Mathf.Lerp(-spread * 0.5f, spread * 0.5f, i / (float)(count - 1)) : 0f;
                Vector3 rotated = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                caster.Caster.Projectiles.Launch(launch.Projectile, context.Rank, caster.Position, rotated, launch.Projectile.Homing ? context.Target : null);
            }
        }

        private static Vector3 Flat(Vector3 vector)
        {
            vector.y = 0f;
            return vector.sqrMagnitude > 0.0001f ? vector.normalized : Vector3.forward;
        }
    }
}
