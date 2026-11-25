using System;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    [Serializable]
    public sealed class PlaceWardEffect : EffectDefinition
    {
        [SerializeField] private UnitDefinition _ward;
        [SerializeField, Min(0f), Tooltip("0 = until destroyed.")] private float _lifetimeSeconds = 90f;
        [SerializeField, Min(1), Tooltip("Oldest one of the same kind is removed beyond this count.")] private int _maxPerOwner = 3;

        public UnitDefinition Ward => _ward;
        public float LifetimeSeconds => _lifetimeSeconds;
        public int MaxPerOwner => _maxPerOwner;

        public override void Validate(string ownerId, int maxRank, ValidationReport report)
        {
            if (_ward == null) report.Error(ownerId, "Place ward effect has no ward unit.");
        }
    }
}
