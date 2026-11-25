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
        [SerializeField, Tooltip("Counts towards the dragon soul.")] private bool _isDrake;
        [SerializeField] private StatusEffectDefinition _soulReward;
        [SerializeField, Tooltip("Spawns at the average champion level.")] private bool _scalesWithAverageLevel;

        public bool IsEpic => _isEpic;
        public StatusEffectDefinition KillerReward => _killerReward;
        public StatusEffectDefinition TeamReward => _teamReward;
        public int TeamGold => _teamGold;
        public bool IsDrake => _isDrake;
        public StatusEffectDefinition SoulReward => _soulReward;
        public bool ScalesWithAverageLevel => _scalesWithAverageLevel;

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            if (Kind != UnitKind.Monster) report.Error(Id, "A monster definition must have Kind = Monster.");
        }
    }
}
