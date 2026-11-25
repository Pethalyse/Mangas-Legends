using System;
using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Effects
{
    // Kills the target outright when its health ratio is at or under the threshold.
    [Serializable]
    public sealed class ExecuteEffect : EffectDefinition
    {
        [SerializeField, Range(0f, 1f)] private float _healthRatio = 0.2f;
        [SerializeField] private bool _championsOnly = true;

        public float HealthRatio => _healthRatio;
        public bool ChampionsOnly => _championsOnly;
    }
}
