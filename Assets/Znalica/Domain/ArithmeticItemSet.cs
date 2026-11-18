using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// Every addition <c>a + b = c</c> and subtraction <c>c - a = b</c> with all numbers in 0-20, in all three
    /// unknown positions. Both operand orders of both operations are in it, so the four forms of one fact are.
    /// </summary>
    public sealed class ArithmeticItemSet : IItemSet
    {
        private static readonly Unknown[] Positions = { Unknown.Result, Unknown.First, Unknown.Second };

        private readonly List<Item> _all;

        public ArithmeticItemSet()
        {
            _all = new List<Item>();
            for (int a = NumberRange.Min; a <= NumberRange.Max; a++)
            {
                for (int b = NumberRange.Min; a + b <= NumberRange.Max; b++)
                {
                    int sum = a + b;
                    foreach (Unknown unknown in Positions)
                    {
                        // The constructor refuses a number outside 0-20.
                        _all.Add(new ArithmeticItem(a, Operation.Addition, b, sum, unknown));
                        _all.Add(new ArithmeticItem(sum, Operation.Subtraction, a, b, unknown));
                    }
                }
            }

            ItemOrder.SortEasiestFirst(_all);
        }

        public Activity Activity => Activity.Arithmetic;

        public IReadOnlyList<Item> All => _all;

        public bool TryFind(ItemKey key, out Item item)
        {
            bool found = ArithmeticItem.TryParse(key, out ArithmeticItem arithmetic);
            item = arithmetic;
            return found;
        }
    }
}
