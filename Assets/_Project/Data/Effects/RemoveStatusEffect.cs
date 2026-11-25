using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class RemoveStatusEffect : EffectDefinition
    {
        [SerializeField] private EffectTarget _target = EffectTarget.Caster;
        [SerializeField] private StatusEffectDefinition _status;

        public EffectTarget Target => _target;
        public StatusEffectDefinition Status => _status;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_status == null) report.Error(ownerId, "RemoveStatusEffect has no status assigned.");
        }
    }
}
