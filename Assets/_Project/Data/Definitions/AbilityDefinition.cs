using System.Collections.Generic;
using MangaLegends.Core.Abilities;
using MangaLegends.Core.Content;
using MangaLegends.Data.Effects;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Ability", fileName = "ABIL_New")]
    public sealed class AbilityDefinition : ContentDefinition
    {
        [SerializeField, Range(1, 6)] private int _maxRank = 5;
        [SerializeField] private CastMode _castMode = CastMode.Instant;
        [SerializeField] private float[] _cooldownSeconds = { 10f };
        [SerializeField] private float[] _cost = { 0f };
        [SerializeField, Min(0f)] private float _castTimeSeconds;
        [SerializeField, Min(0f)] private float _channelSeconds;
        [SerializeField, Min(1)] private int _charges = 1;
        [SerializeField] private TargetingData _targeting = new TargetingData();
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onCast = new List<EffectDefinition>();
        [SerializeField] private string _behaviourId;

        public int MaxRank => _maxRank;
        public CastMode CastMode => _castMode;
        public float CastTimeSeconds => _castTimeSeconds;
        public float ChannelSeconds => _channelSeconds;
        public int Charges => _charges;
        public TargetingData Targeting => _targeting;
        public IReadOnlyList<EffectDefinition> OnCast => _onCast;
        public string BehaviourId => _behaviourId;

        public float CooldownAtRank(int rank) => RankValues.At(_cooldownSeconds, rank);
        public float CostAtRank(int rank) => RankValues.At(_cost, rank);

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            report.CheckRankArray(Id, "Cooldown", _cooldownSeconds?.Length ?? 0, _maxRank);
            report.CheckRankArray(Id, "Cost", _cost?.Length ?? 0, _maxRank);
            if (_castMode != CastMode.Passive && _onCast.Count == 0 && string.IsNullOrEmpty(_behaviourId))
                report.Warning(Id, "Ability has no effect and no custom behaviour.");
            foreach (EffectDefinition effect in _onCast)
            {
                if (effect == null) report.Error(Id, "An effect entry is empty.");
                else effect.Validate(Id, _maxRank, report);
            }
        }
    }
}
