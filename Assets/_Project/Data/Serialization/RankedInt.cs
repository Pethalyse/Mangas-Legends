using System;
using MangaLegends.Core.Content;
using UnityEngine;

namespace MangaLegends.Data.Serialization
{
    [Serializable]
    public sealed class RankedInt
    {
        [SerializeField] private int[] _byRank;

        public RankedInt(params int[] byRank)
        {
            _byRank = byRank != null && byRank.Length > 0 ? byRank : new[] { 0 };
        }

        public int Count => _byRank?.Length ?? 0;

        public int At(int rank)
        {
            if (Count == 0) return 0;
            int index = Math.Min(Math.Max(rank, 1), _byRank.Length) - 1;
            return _byRank[index];
        }

        public void Validate(string ownerId, string field, int maxRank, ValidationReport report) =>
            report.CheckRankArray(ownerId, field, Count, maxRank);
    }
}
