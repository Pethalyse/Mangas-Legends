using System;

namespace MangaLegends.Data.Definitions
{
    [Flags]
    public enum ChampionTraits
    {
        None = 0,
        DevilFruitUser = 1 << 0
    }
}
