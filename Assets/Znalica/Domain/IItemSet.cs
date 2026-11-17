using System.Collections.Generic;

namespace Znalica.Domain
{
    public interface IItemSet
    {
        Activity Activity { get; }

        /// <summary>Every item of the set, easiest first, then by key, so the order is the same every run.</summary>
        IReadOnlyList<Item> All { get; }

        /// <summary>
        /// Builds the item a key names. False for a key that does not parse, belongs to another activity, holds a
        /// number outside 0-20, or names an item that cannot exist (for example one whose missing number would be
        /// below 0). Such a key comes from a log line written by another version; the caller skips it.
        /// </summary>
        bool TryFind(ItemKey key, out Item item);
    }
}
