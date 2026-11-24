using System;
using MangaLegends.Core.Content;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public abstract class EffectDefinition
    {
        public virtual void Validate(string ownerId, int maxRank, ValidationReport report)
        {
        }
    }

    public enum EffectTarget
    {
        Target,
        Caster,
        AllHitUnits
    }
}
