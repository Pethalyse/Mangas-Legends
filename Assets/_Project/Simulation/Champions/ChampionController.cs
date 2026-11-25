using Fusion;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Input;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Champions
{
    [RequireComponent(typeof(Unit), typeof(ChampionProgress), typeof(Inventory))]
    public sealed class ChampionController : NetworkBehaviour, IUnitBrain
    {

        private Unit _unit;
        private Inventory _inventory;

        [Networked] public NetworkBool AttackMoving { get; set; }

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
            if (HasStateAuthority) _unit.Services.Match?.UpdateHomeguard(_unit);
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
            var trinkets = _unit.Services.Rules.TrinketChoices;
            if (input.TrinketChoice >= 0 && input.TrinketChoice < trinkets.Count && _inventory.CanShop) _unit.Caster.ChooseTrinket(trinkets[input.TrinketChoice]);
        }

        private void HandleLevelUp(in PlayerInput input)
        {
            if (input.LevelUpSlot >= 0 && _unit.Caster != null) _unit.Caster.LevelUp((CasterSlot)input.LevelUpSlot);
        }

        private void HandleMovement(in PlayerInput input)
        {
            if (input.Has(InputButton.Stop))
            {
                AttackMoving = false;
                _unit.Motor.Stop();
                _unit.Attack.ClearTarget();
                _unit.Caster.CancelPending();
                return;
            }

            if (input.Has(InputButton.AttackMove))
            {
                Unit attackMoveTarget = AttackMoveTarget(input.MovePoint);
                if (attackMoveTarget != null)
                {
                    AttackMoving = false;
                    BeginAttack(attackMoveTarget);
                    return;
                }
                if (_unit.Motor.IsForcedMovement) return;
                _unit.Caster.CancelChannel();
                _unit.Attack.ClearTarget();
                _unit.Motor.MoveTo(input.MovePoint);
                AttackMoving = true;
                return;
            }

            if (!input.Has(InputButton.Move))
            {
                ContinueAttackMove();
                return;
            }
            AttackMoving = false;
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

        // LoL attack-move: the enemy near the click, else the one in attack range closest to the click,
        // else walk to the click and attack the first enemy that comes into range.
        private Unit AttackMoveTarget(Vector3 point)
        {
            Unit nearClick = _unit.Services.Units.Closest(point, _unit.Services.Rules.AttackMoveRadius, IsAttackMoveTarget);
            if (nearClick != null) return nearClick;
            Unit best = null;
            float bestDistance = float.MaxValue;
            foreach (Unit unit in _unit.Services.Units.All)
            {
                if (unit == null || !IsAttackMoveTarget(unit) || !_unit.Attack.IsInRange(unit)) continue;
                float distance = UnitRegistry.FlatDistance(unit.Position, point);
                if (distance < bestDistance) { best = unit; bestDistance = distance; }
            }
            return best;
        }

        private void ContinueAttackMove()
        {
            if (!AttackMoving) return;
            if (_unit.Attack.HasTarget) return;
            if (!_unit.Motor.IsMoving)
            {
                AttackMoving = false;
                return;
            }
            Unit closest = _unit.Services.Units.Closest(_unit.Position, _unit.Attack.Range + _unit.Radius + 1f, u => IsAttackMoveTarget(u) && _unit.Attack.IsInRange(u));
            if (closest != null) BeginAttack(closest);
        }

        private bool IsAttackMoveTarget(Unit unit) =>
            unit.IsAlive && _unit.IsEnemyOf(unit) && unit.IsTargetable && !unit.IsInvulnerable && unit.Kind != UnitKind.Ward;

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
            Cast(input, InputButton.Trinket, CasterSlot.Trinket);
            for (InputButton button = InputButton.Item1; button <= InputButton.Item6; button++)
            {
                if (!input.Has(button)) continue;
                _unit.TryResolve(input.AimTarget, out Unit aimTarget);
                _inventory.TryUseActive(AimSource.ItemSlotOf(button), input.AimPoint, aimTarget);
            }
        }

        private void Cast(in PlayerInput input, InputButton button, CasterSlot slot)
        {
            if (!input.Has(button)) return;
            _unit.TryResolve(input.AimTarget, out Unit aimTarget);
            _unit.Caster.TryCast(slot, input.AimPoint, aimTarget);
        }
    }
}
