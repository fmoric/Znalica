using System;
using System.Collections.Generic;
using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// The one place that turns an item into what is shown: a list of tokens, so the screen can draw the unknown as
    /// a box. Keys use the ASCII <c>-</c>; the screen shows the minus sign U+2212. Later steps add the other
    /// activities' items and the progress view's use of keys.
    /// </summary>
    public static class ItemFormatter
    {
        public const string MinusSign = "−";
        public const string PlusSign = "+";
        public const string EqualsSign = "=";
        public const string LessSign = "<";
        public const string GreaterSign = ">";

        /// <summary>The sign as drawn on a comparison button and between two quantities.</summary>
        public static string SignText(Sign sign)
        {
            switch (sign)
            {
                case Sign.Less: return LessSign;
                case Sign.Greater: return GreaterSign;
                default: return EqualsSign;
            }
        }

        /// <summary><c>8 + 5 = ?</c> as five tokens: first, operation, second, equals sign, result.</summary>
        /// <exception cref="ArgumentException">An item that is not an arithmetic item.</exception>
        public static IReadOnlyList<ItemToken> Tokens(Item item)
        {
            if (!(item is ArithmeticItem arithmetic))
            {
                throw new ArgumentException("Only arithmetic items can be formatted so far.", nameof(item));
            }

            return new[]
            {
                Number(arithmetic.First, arithmetic.Unknown == Unknown.First),
                new ItemToken(arithmetic.Operation == Operation.Addition ? PlusSign : MinusSign, false),
                Number(arithmetic.Second, arithmetic.Unknown == Unknown.Second),
                new ItemToken(EqualsSign, false),
                Number(arithmetic.Result, arithmetic.Unknown == Unknown.Result),
            };
        }

        /// <summary>The size of a jump on the number line: <c>+5</c>, or <c>−6</c> for a jump back (<c>−0</c> for taking 0 away).</summary>
        public static string ChangeText(Explanation explanation)
        {
            return (explanation.Backwards ? MinusSign : PlusSign)
                + Math.Abs(explanation.Change).ToString(CultureInfo.InvariantCulture);
        }

        private static ItemToken Number(int number, bool unknown)
        {
            return new ItemToken(unknown ? ArithmeticItem.UnknownMark : number.ToString(CultureInfo.InvariantCulture), unknown);
        }
    }
}
