using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Monster", fileName = "MONSTER_New")]
    public sealed class MonsterDefinition : UnitDefinition
    {
        [SerializeField] private bool _isEpic;
        [SerializeField] private StatusEffectDefinition _killerReward;
        [SerializeField] private StatusEffectDefinition _teamReward;
        [SerializeField, Min(0)] private int _teamGold;

        public bool IsEpic => _isEpic;
        public StatusEffectDefinition KillerReward => _killerReward;
        public StatusEffectDefinition TeamReward => _teamReward;
        public int TeamGold => _teamGold;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (Kind != UnitKind.Monster) report.Error(Id, "A monster definition must have Kind = Monster.");
        }
    }
}
