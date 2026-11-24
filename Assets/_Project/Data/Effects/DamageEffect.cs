using System;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class DamageEffect : EffectDefinition
    {
        [SerializeField] private EffectTarget _target = EffectTarget.Target;
        [SerializeField] private DamageType _type = DamageType.Physical;
        [SerializeField] private ScalingValueData _amount = new ScalingValueData();
        [SerializeField] private bool _canCrit;
        [SerializeField] private bool _appliesOnHitEffects;

        public EffectTarget Target => _target;
        public DamageType Type => _type;
        public ScalingValueData Amount => _amount;
        public bool CanCrit => _canCrit;
        public bool AppliesOnHitEffects => _appliesOnHitEffects;

        public override void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _amount.Validate(ownerId, "Damage amount", maxRank, report);
    }
}
