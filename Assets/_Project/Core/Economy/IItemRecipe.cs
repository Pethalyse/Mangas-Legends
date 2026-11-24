using System.Collections.Generic;

namespace MangaLegends.Core.Economy
{
    public interface IItemRecipe
    {
        string Id { get; }
        int RecipeCost { get; }
        bool IsUnique { get; }
        IReadOnlyList<IItemRecipe> Components { get; }
    }
}
