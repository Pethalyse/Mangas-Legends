using System.Collections.Generic;
using MangaLegends.Simulation.Units;
using UnityEngine;

namespace MangaLegends.Simulation.Abilities
{
    public readonly struct EffectContext
    {
        public EffectContext(Unit caster, Unit target, Vector3 aimPoint, Vector3 impactPoint, int rank, IReadOnlyList<Unit> hitUnits)
        {
            Caster = caster;
            Target = target;
            AimPoint = aimPoint;
            ImpactPoint = impactPoint;
            Rank = rank < 1 ? 1 : rank;
            HitUnits = hitUnits;
        }

        public Unit Caster { get; }
        public Unit Target { get; }
        public Vector3 AimPoint { get; }
        public Vector3 ImpactPoint { get; }
        public int Rank { get; }
        public IReadOnlyList<Unit> HitUnits { get; }

        public Vector3 AimDirection
        {
            get
            {
                Vector3 direction = AimPoint - Caster.Position;
                direction.y = 0f;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : Caster.transform.forward;
            }
        }

        public static EffectContext ForTarget(Unit caster, Unit target, int rank) =>
            new EffectContext(caster, target, target != null ? target.Position : caster.Position, target != null ? target.Position : caster.Position, rank, null);

        public EffectContext WithHits(Unit target, Vector3 impactPoint, IReadOnlyList<Unit> hits) =>
            new EffectContext(Caster, target, AimPoint, impactPoint, Rank, hits);
    }
}
