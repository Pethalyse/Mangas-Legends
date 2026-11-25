using System;

namespace MangaLegends.Core.Combat
{
    [Flags]
    public enum DamageFlags
    {
        None = 0,
        AutoAttack = 1 << 0,
        Ability = 1 << 1,
        Critical = 1 << 2,
        Periodic = 1 << 3,
        IgnoreShields = 1 << 4,
        Proc = 1 << 5
    }
}
