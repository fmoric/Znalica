using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>The order every item set lists its items in, so it is the same every run.</summary>
    public static class ItemOrder
    {
        /// <summary>Easiest <see cref="Difficulty"/> first, then by key.</summary>
        public static void SortEasiestFirst(List<Item> items)
        {
            items.Sort((x, y) =>
            {
                int byDifficulty = x.Difficulty.CompareTo(y.Difficulty);
                return byDifficulty != 0 ? byDifficulty : string.CompareOrdinal(x.Key.ToString(), y.Key.ToString());
            });
        }
    }
}
