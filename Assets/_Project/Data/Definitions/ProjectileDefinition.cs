using System.Collections.Generic;
using MangaLegends.Core.Content;
using MangaLegends.Data.Effects;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Projectile", fileName = "PROJ_New")]
    public sealed class ProjectileDefinition : ContentDefinition
    {
        [SerializeField, Min(0.1f)] private float _speed = 15f;
        [SerializeField, Min(0.1f)] private float _maxDistance = 10f;
        [SerializeField, Min(0.05f)] private float _radius = 0.5f;
        [SerializeField] private bool _homing;
        [SerializeField, Min(1)] private int _maxHits = 1;
        [SerializeField] private TeamFilter _hits = TeamFilter.Enemies;
        [SerializeField] private bool _stopsOnTerrain = true;
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onHit = new List<EffectDefinition>();
        [SerializeField] private GameObject _visualPrefab;

        public float Speed => _speed;
        public float MaxDistance => _maxDistance;
        public float Radius => _radius;
        public bool Homing => _homing;
        public int MaxHits => _maxHits;
        public TeamFilter Hits => _hits;
        public bool StopsOnTerrain => _stopsOnTerrain;
        public IReadOnlyList<EffectDefinition> OnHit => _onHit;
        public GameObject VisualPrefab => _visualPrefab;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (_onHit.Count == 0) report.Warning(Id, "Projectile has no on-hit effect.");
            foreach (EffectDefinition effect in _onHit)
            {
                if (effect == null) report.Error(Id, "An effect entry is empty.");
                else effect.Validate(Id, 1, report);
            }
        }
    }
}
