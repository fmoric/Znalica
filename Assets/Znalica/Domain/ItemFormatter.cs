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

        /// <summary>
        /// <c>8 + 5 = ?</c> as five tokens: first, operation, second, equals sign, result. A number-line item is its task as
        /// the child sees it: the number to place, <c>? , 14</c> and <c>14 , ?</c> for before and after, and
        /// <c>6 +3 = ?</c> for a jump.
        /// </summary>
        /// <exception cref="ArgumentException">An item that is neither an arithmetic nor a number-line item.</exception>
        public static IReadOnlyList<ItemToken> Tokens(Item item)
        {
            if (item is NumberLineItem numberLine)
            {
                return Tokens(numberLine);
            }

            if (!(item is ArithmeticItem arithmetic))
            {
                throw new ArgumentException("Only arithmetic and number-line items can be formatted so far.", nameof(item));
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
            return JumpText(explanation.Backwards, explanation.Change);
        }

        private static IReadOnlyList<ItemToken> Tokens(NumberLineItem item)
        {
            var unknown = new ItemToken(ArithmeticItem.UnknownMark, true);
            var comma = new ItemToken(",", false);
            switch (item.Task)
            {
                case NumberLineTask.Before:
                    return new[] { unknown, comma, Number(item.Target, false) };
                case NumberLineTask.After:
                    return new[] { Number(item.Target, false), comma, unknown };
                case NumberLineTask.Jump:
                    return new[]
                    {
                        Number(item.Target, false),
                        new ItemToken(JumpText(item.Step < 0, item.Step), false),
                        new ItemToken(EqualsSign, false),
                        unknown,
                    };
                default:
                    return new[] { Number(item.Target, false) };
            }
        }

        private static string JumpText(bool backwards, int change)
        {
            return (backwards ? MinusSign : PlusSign) + Math.Abs(change).ToString(CultureInfo.InvariantCulture);
        }

        private static ItemToken Number(int number, bool unknown)
        {
            return new ItemToken(unknown ? ArithmeticItem.UnknownMark : number.ToString(CultureInfo.InvariantCulture), unknown);
        }
    }
}
