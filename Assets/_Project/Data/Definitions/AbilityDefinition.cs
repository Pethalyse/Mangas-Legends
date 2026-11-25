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
        [SerializeField] private float[] _costPerSecond = { 0f };
        [SerializeField] private RankedFloat _castTimeSeconds = new RankedFloat(0f);
        [SerializeField] private RankedFloat _channelSeconds = new RankedFloat(0f);
        [SerializeField] private RankedInt _charges = new RankedInt(1);
        [SerializeField] private TargetingData _targeting = new TargetingData();
        [SerializeReference, TypeSelector] private List<EffectDefinition> _onCast = new List<EffectDefinition>();
        [SerializeField] private string _behaviourId;
        [SerializeField, Tooltip("Passive only: status kept on the champion while the passive is ready; its stacks are this base plus the champion level.")] private StatusEffectDefinition _readyStatus;
        [SerializeField, Min(0)] private int _readyStatusBaseStacks;

        public int MaxRank => _maxRank;
        public CastMode CastMode => _castMode;
        public float CastTimeSecondsAt(int rank) => _castTimeSeconds.At(rank);
        public float ChannelSecondsAt(int rank) => _channelSeconds.At(rank);
        public int ChargesAt(int rank) => System.Math.Max(1, _charges.At(rank));
        public TargetingData Targeting => _targeting;
        public IReadOnlyList<EffectDefinition> OnCast => _onCast;
        public string BehaviourId => _behaviourId;
        public StatusEffectDefinition ReadyStatus => _readyStatus;
        public int ReadyStatusBaseStacks => _readyStatusBaseStacks;

        public float CooldownAtRank(int rank) => RankValues.At(_cooldownSeconds, rank);
        public float CostAtRank(int rank) => RankValues.At(_cost, rank);
        public float CostPerSecondAtRank(int rank) => RankValues.At(_costPerSecond, rank);

        public override void Validate(ValidationReport report)
        {
            base.Validate(report);
            report.CheckRankArray(Id, "Cooldown", _cooldownSeconds?.Length ?? 0, _maxRank);
            report.CheckRankArray(Id, "Cost", _cost?.Length ?? 0, _maxRank);
            report.CheckRankArray(Id, "CostPerSecond", _costPerSecond?.Length ?? 0, _maxRank);
            _castTimeSeconds.Validate(Id, "Cast time", _maxRank, report);
            _channelSeconds.Validate(Id, "Channel time", _maxRank, report);
            _charges.Validate(Id, "Charges", _maxRank, report);
            _targeting.Validate(Id, _maxRank, report);
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
