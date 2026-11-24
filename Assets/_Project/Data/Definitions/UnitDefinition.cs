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

        public UnitKind Kind => _kind;
        public StatProfile Stats => _stats;
        public GameObject Prefab => _prefab;
        public int GoldBounty => _goldBounty;
        public int ExperienceBounty => _experienceBounty;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            _stats.Validate(Id, report);
            if (_prefab == null) report.Warning(Id, "No prefab assigned.");
        }
    }
}
