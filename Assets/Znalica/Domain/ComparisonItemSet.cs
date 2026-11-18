using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// Every sign item with both numbers in 0-20, with numbers and with groups. The ordering items are not in
    /// <see cref="All"/> (their space is too large to list): <see cref="TryFind"/> builds one from its key, and
    /// <see cref="ComparisonRoundPlanner"/> draws new ones.
    /// </summary>
    public sealed class ComparisonItemSet : IItemSet
    {
        private readonly List<Item> _all = new List<Item>();

        public ComparisonItemSet()
        {
            for (int left = NumberRange.Min; left <= NumberRange.Max; left++)
            {
                for (int right = NumberRange.Min; right <= NumberRange.Max; right++)
                {
                    _all.Add(ComparisonItem.ChooseSign(Representation.Numbers, left, right));
                    _all.Add(ComparisonItem.ChooseSign(Representation.Groups, left, right));
                }
            }

            ItemOrder.SortEasiestFirst(_all);
        }

        public Activity Activity => Activity.Comparison;

        public IReadOnlyList<Item> All => _all;

        public bool TryFind(ItemKey key, out Item item)
        {
            bool found = ComparisonItem.TryParse(key, out ComparisonItem comparison);
            item = comparison;
            return found;
        }
    }
}
