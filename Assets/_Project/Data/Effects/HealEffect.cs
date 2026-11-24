using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    public enum HealedPool
    {
        Health,
        Resource
    }

    [Serializable]
    public sealed class HealEffect : EffectDefinition
    {
        [SerializeField] private EffectTarget _target = EffectTarget.Target;
        [SerializeField] private HealedPool _pool = HealedPool.Health;
        [SerializeField] private ScalingValueData _amount = new ScalingValueData();

        public EffectTarget Target => _target;
        public HealedPool Pool => _pool;
        public ScalingValueData Amount => _amount;

        public override void Validate(string ownerId, int maxRank, ValidationReport report) =>
            _amount.Validate(ownerId, "Heal amount", maxRank, report);
    }
}
