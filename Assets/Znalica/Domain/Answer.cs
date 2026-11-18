using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>
    /// What the child answered: a number (arithmetic), a sign (comparison) or the numbers in the order they were
    /// clicked (ordering). Exactly one is set; the activity decides which.
    /// </summary>
    public readonly struct Answer
    {
        private readonly int[] _order;

        private Answer(int? number, Sign? sign, int[] order)
        {
            Number = number;
            Sign = sign;
            _order = order;
        }

        public int? Number { get; }

        public Sign? Sign { get; }

        /// <summary>The numbers in the order they were chosen; null unless this is an ordering answer.</summary>
        public IReadOnlyList<int> Order => _order == null ? null : Array.AsReadOnly(_order);

        public static Answer FromNumber(int number)
        {
            return new Answer(number, null, null);
        }

        public static Answer FromSign(Sign sign)
        {
            return new Answer(null, sign, null);
        }

        public static Answer FromOrder(IEnumerable<int> order)
        {
            return new Answer(null, null, (order ?? throw new ArgumentNullException(nameof(order))).ToArray());
        }

        /// <summary>The form kept in the answer log: <c>13</c>, <c>Less</c>, <c>3,9,14</c>.</summary>
        public string Canonical()
        {
            if (Number.HasValue)
            {
                return Number.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (Sign.HasValue)
            {
                return Sign.Value.ToString();
            }

            return _order == null
                ? string.Empty
                : string.Join(",", _order.Select(n => n.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
