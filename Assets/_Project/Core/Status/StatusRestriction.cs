using System;

namespace MangaLegends.Core.Status
{
    [Flags]
    public enum StatusRestriction
    {
        None = 0,
        Move = 1 << 0,
        Cast = 1 << 1,
        Attack = 1 << 2,
        BeTargeted = 1 << 3,
        UseItems = 1 << 4
    }
}
