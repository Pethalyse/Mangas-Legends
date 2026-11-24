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
        [SerializeField] private RankedFloat _speed = new RankedFloat(15f);
        [SerializeField] private RankedFloat _maxDistance = new RankedFloat(10f);
        [SerializeField] private RankedFloat _radius = new RankedFloat(0.5f);
        [SerializeField] private bool _homing;
        [SerializeField] private RankedInt _maxHits = new RankedInt(1);
        [SerializeField] private TeamFilter _hits = TeamFilter.Enemies;
        [SerializeField] private bool _stopsOnTerrain = true;
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onHit = new List<EffectDefinition>();
        [SerializeField] private GameObject _visualPrefab;

        public float SpeedAt(int rank) => _speed.At(rank);
        public float MaxDistanceAt(int rank) => _maxDistance.At(rank);
        public float RadiusAt(int rank) => _radius.At(rank);
        public bool Homing => _homing;
        public int MaxHitsAt(int rank) => System.Math.Max(1, _maxHits.At(rank));
        public TeamFilter Hits => _hits;
        public bool StopsOnTerrain => _stopsOnTerrain;
        public IReadOnlyList<EffectDefinition> OnHit => _onHit;
        public GameObject VisualPrefab => _visualPrefab;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (_onHit.Count == 0) report.Warning(Id, "Projectile has no on-hit effect.");
            foreach (EffectDefinition effect in _onHit)
                if (effect == null) report.Error(Id, "An effect entry is empty.");
        }

        public void ValidateForOwner(string ownerId, int maxRank, ValidationReport report)
        {
            string field = $"Projectile {Id}";
            _speed.Validate(ownerId, field + " speed", maxRank, report);
            _maxDistance.Validate(ownerId, field + " distance", maxRank, report);
            _radius.Validate(ownerId, field + " radius", maxRank, report);
            _maxHits.Validate(ownerId, field + " hits", maxRank, report);
            foreach (EffectDefinition effect in _onHit) effect?.Validate(ownerId, maxRank, report);
        }
    }
}
