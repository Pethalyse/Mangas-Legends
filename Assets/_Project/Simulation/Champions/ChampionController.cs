using Fusion;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Input;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Champions
{
    [RequireComponent(typeof(Unit), typeof(ChampionProgress), typeof(Inventory))]
    public sealed class ChampionController : NetworkBehaviour, IUnitBrain
    {
        private const float AttackMoveSearchRadius = 4f;

        private Unit _unit;
        private Inventory _inventory;

        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _inventory = GetComponent<Inventory>();
        }

        public override void FixedUpdateNetwork()
        {
            if (_unit.IsAlive || !GetInput(out PlayerInput input)) return;
            HandleShopping(input);
            HandleLevelUp(input);
        }

        public void Think()
        {
            if (!GetInput(out PlayerInput input)) return;
            HandleShopping(input);
            HandleLevelUp(input);
            HandleMovement(input);
            HandleCasts(input);
        }

        private void HandleShopping(in PlayerInput input)
        {
            if (!HasStateAuthority) return;
            if (input.BuyItem >= 0) _inventory.TryBuy(input.BuyItem);
            if (input.SellSlot >= 0) _inventory.TrySell(input.SellSlot);
        }

        private void HandleLevelUp(in PlayerInput input)
        {
            if (input.LevelUpSlot >= 0 && _unit.Caster != null) _unit.Caster.LevelUp((CasterSlot)input.LevelUpSlot);
        }

        private void HandleMovement(in PlayerInput input)
        {
            if (input.Has(InputButton.Stop))
            {
                _unit.Motor.Stop();
                _unit.Attack.ClearTarget();
                _unit.Caster.CancelPending();
                return;
            }

            if (input.Has(InputButton.AttackMove))
            {
                Unit closest = _unit.Services.Units.Closest(input.MovePoint, AttackMoveSearchRadius, u => _unit.IsEnemyOf(u) && u.IsTargetable && !u.IsInvulnerable);
                if (closest != null)
                {
                    BeginAttack(closest);
                    return;
                }
            }

            if (!input.Has(InputButton.Move) && !input.Has(InputButton.AttackMove)) return;
            if (_unit.TryResolve(input.MoveTarget, out Unit target) && _unit.IsEnemyOf(target) && target.IsAlive && target.IsTargetable)
            {
                BeginAttack(target);
                return;
            }
            if (_unit.Motor.IsForcedMovement) return;
            _unit.Caster.CancelChannel();
            _unit.Attack.ClearTarget();
            _unit.Motor.MoveTo(input.MovePoint);
        }

        private void BeginAttack(Unit target)
        {
            _unit.Caster.CancelChannel();
            _unit.Attack.SetTarget(target);
        }

        private void HandleCasts(in PlayerInput input)
        {
            Cast(input, InputButton.AbilityA, CasterSlot.A);
            Cast(input, InputButton.AbilityZ, CasterSlot.Z);
            Cast(input, InputButton.AbilityE, CasterSlot.E);
            Cast(input, InputButton.AbilityR, CasterSlot.R);
            Cast(input, InputButton.Summoner1, CasterSlot.Summoner1);
            Cast(input, InputButton.Summoner2, CasterSlot.Summoner2);
            Cast(input, InputButton.Recall, CasterSlot.Recall);
        }

        private void Cast(in PlayerInput input, InputButton button, CasterSlot slot)
        {
            if (!input.Has(button)) return;
            _unit.TryResolve(input.AimTarget, out Unit aimTarget);
            _unit.Caster.TryCast(slot, input.AimPoint, aimTarget);
        }
    }
}
