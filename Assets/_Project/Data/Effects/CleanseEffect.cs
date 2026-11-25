using System;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class CleanseEffect : EffectDefinition
    {
        [SerializeField] private EffectTarget _target = EffectTarget.Caster;

        public EffectTarget Target => _target;
    }
}
