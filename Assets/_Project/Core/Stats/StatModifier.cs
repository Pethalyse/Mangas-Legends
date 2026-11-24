using System;

namespace MangaLegends.Core.Stats
{
    public readonly struct StatModifier : IEquatable<StatModifier>
    {
        public StatModifier(StatType stat, ModifierKind kind, float value, int sourceId)
        {
            Stat = stat;
            Kind = kind;
            Value = value;
            SourceId = sourceId;
        }

        public StatType Stat { get; }
        public ModifierKind Kind { get; }
        public float Value { get; }
        public int SourceId { get; }

        public bool Equals(StatModifier other) =>
            Stat == other.Stat && Kind == other.Kind && Value.Equals(other.Value) && SourceId == other.SourceId;

        public override bool Equals(object obj) => obj is StatModifier other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Stat, (int)Kind, Value, SourceId);
    }
}
