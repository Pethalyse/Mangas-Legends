using System;
using MangaLegends.Core.Abilities;
using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public sealed class RankedFloat
    {
        [SerializeField] private float[] _byRank;

        public RankedFloat(params float[] byRank)
        {
            _byRank = byRank != null && byRank.Length > 0 ? byRank : new[] { 0f };
        }

        public int Count => _byRank?.Length ?? 0;

        public float At(int rank) => RankValues.At(_byRank, rank);

        public void Validate(string ownerId, string field, int maxRank, ValidationReport report) =>
            report.CheckRankArray(ownerId, field, Count, maxRank);
    }
}
