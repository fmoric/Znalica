using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// Every Brojevna crta item: place 0-20, the number before 1-20, the number after 0-19, and every jump that starts
    /// and lands in 0-20 and is 1 to <see cref="NumberLineItem.MaxJump"/> long, forwards or back.
    /// </summary>
    public sealed class NumberLineItemSet : IItemSet
    {
        private readonly List<Item> _all = new List<Item>();

        public NumberLineItemSet()
        {
            for (int number = NumberRange.Min; number <= NumberRange.Max; number++)
            {
                _all.Add(NumberLineItem.Place(number));
                if (number > NumberRange.Min)
                {
                    _all.Add(NumberLineItem.Before(number));
                }

                if (number < NumberRange.Max)
                {
                    _all.Add(NumberLineItem.After(number));
                }

                for (int step = -NumberLineItem.MaxJump; step <= NumberLineItem.MaxJump; step++)
                {
                    if (step != 0 && NumberRange.Contains(number + step))
                    {
                        _all.Add(NumberLineItem.Jump(number, step));
                    }
                }
            }

            ItemOrder.SortEasiestFirst(_all);
        }

        public Activity Activity => Activity.NumberLine;

        public IReadOnlyList<Item> All => _all;

        public bool TryFind(ItemKey key, out Item item)
        {
            bool found = NumberLineItem.TryParse(key, out NumberLineItem numberLine);
            item = numberLine;
            return found;
        }
    }
}
