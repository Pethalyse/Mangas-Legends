using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Unit", fileName = "UNIT_New")]
    public class UnitDefinition : ContentDefinition
    {
        [SerializeField] private UnitKind _kind = UnitKind.Minion;
        [SerializeField] private StatProfile _stats = new StatProfile();
        [SerializeField] private GameObject _prefab;
        [SerializeField, Min(0)] private int _goldBounty;
        [SerializeField, Min(0)] private int _experienceBounty;

        [Header("Body")]
        [SerializeField, Min(0.1f)] private float _collisionRadius = 0.5f;
        [SerializeField] private Color _tint = Color.white;

        [Header("Basic attack")]
        [SerializeField, Min(0f)] private float _attackProjectileSpeed;
        [SerializeField, Range(0f, 1f)] private float _attackWindup = 0.3f;

        public UnitKind Kind => _kind;
        public StatProfile Stats => _stats;
        public GameObject Prefab => _prefab;
        public int GoldBounty => _goldBounty;
        public int ExperienceBounty => _experienceBounty;
        public float CollisionRadius => _collisionRadius;
        public Color Tint => _tint;
        public float AttackProjectileSpeed => _attackProjectileSpeed;
        public bool IsRanged => _attackProjectileSpeed > 0f;
        public float AttackWindup => _attackWindup;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            _stats.Validate(Id, report);
            if (_prefab == null) report.Warning(Id, "No prefab assigned.");
        }
    }
}
