using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>
    /// An item of Veće, manje, jednako: choose the sign between two quantities (<c>C:7?12</c> with numbers,
    /// <c>C:G7?12</c> with groups), or put three to five numbers in order (<c>O:3,9,14</c>, sorted, so one set has one
    /// key). Every number lies in 0-20.
    /// </summary>
    public sealed class ComparisonItem : Item
    {
        public const int MinOrderCount = 3;
        public const int MaxOrderCount = 5;

        private const string SignPrefix = "C:";
        private const string OrderPrefix = "O:";
        private const string GroupMark = "G";
        private const char SignSeparator = '?';
        private const char OrderSeparator = ',';

        // The order the numbers of a PutInOrder item are shown in, by how many there are: a fixed shuffle, so a rebuilt
        // item looks the same and is never already in order. Entry i is the index into the ascending numbers.
        private static readonly int[][] ShownOrder =
        {
            null,
            null,
            null,
            new[] { 2, 0, 1 },
            new[] { 2, 0, 3, 1 },
            new[] { 3, 1, 4, 0, 2 },
        };

        private readonly int[] _numbers;

        private ComparisonItem(ComparisonTask task, Representation representation, int[] numbers)
        {
            Task = task;
            Representation = representation;
            _numbers = numbers;
            Key = ItemKey.Of(Format());
            Difficulty = numbers.All(n => n <= 10) ? Difficulty.WithinTen : Difficulty.BeyondTenWithoutCrossing;
        }

        public ComparisonTask Task { get; }

        public Representation Representation { get; }

        /// <summary>
        /// The numbers of the item: the left and the right one for <see cref="ComparisonTask.ChooseSign"/>, the whole set,
        /// ascending (the right answer), for <see cref="ComparisonTask.PutInOrder"/>.
        /// </summary>
        public IReadOnlyList<int> Numbers => _numbers;

        /// <summary>The numbers in the order they are shown: for PutInOrder a fixed shuffle of <see cref="Numbers"/>.</summary>
        public IReadOnlyList<int> ShownNumbers
        {
            get
            {
                if (Task == ComparisonTask.ChooseSign)
                {
                    return _numbers;
                }

                return ShownOrder[_numbers.Length].Select(index => _numbers[index]).ToArray();
            }
        }

        public override ItemKey Key { get; }

        public override Activity Activity => Activity.Comparison;

        public override Difficulty Difficulty { get; }

        /// <summary>
        /// A sign item's fact is the pair of numbers, whichever is on the left and however they are shown (<c>7?12</c> for
        /// 7 and 12 in either order, with numbers or with groups). An ordering item is its own fact.
        /// </summary>
        public override string FactId
        {
            get
            {
                if (Task == ComparisonTask.PutInOrder)
                {
                    return base.FactId;
                }

                return Math.Min(_numbers[0], _numbers[1]).ToString(CultureInfo.InvariantCulture) + SignSeparator
                    + Math.Max(_numbers[0], _numbers[1]).ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>The right sign of a <see cref="ComparisonTask.ChooseSign"/> item.</summary>
        public Sign ExpectedSign => SignOf(_numbers[0], _numbers[1]);

        public static Sign SignOf(int left, int right)
        {
            return left < right ? Sign.Less : left > right ? Sign.Greater : Sign.Equal;
        }

        /// <exception cref="ArgumentException">A number outside 0-20.</exception>
        public static ComparisonItem ChooseSign(Representation representation, int left, int right)
        {
            if (!Enum.IsDefined(typeof(Representation), representation))
            {
                throw new ArgumentException("Not a representation.", nameof(representation));
            }

            if (!NumberRange.Contains(left) || !NumberRange.Contains(right))
            {
                throw new ArgumentException("The numbers " + left + " and " + right + " are not both in "
                    + NumberRange.Min + "-" + NumberRange.Max + ".");
            }

            return new ComparisonItem(ComparisonTask.ChooseSign, representation, new[] { left, right });
        }

        /// <summary>The numbers in any order; the item holds them ascending.</summary>
        /// <exception cref="ArgumentException">Fewer than three or more than five numbers, a repeated one, or one outside 0-20.</exception>
        public static ComparisonItem PutInOrder(IEnumerable<int> numbers)
        {
            int[] sorted = (numbers ?? throw new ArgumentNullException(nameof(numbers))).OrderBy(n => n).ToArray();
            if (sorted.Length < MinOrderCount || sorted.Length > MaxOrderCount)
            {
                throw new ArgumentException("An ordering item holds " + MinOrderCount + " to " + MaxOrderCount + " numbers.");
            }

            if (sorted.Any(n => !NumberRange.Contains(n)) || sorted.Distinct().Count() != sorted.Length)
            {
                throw new ArgumentException("The numbers must be different and all in " + NumberRange.Min + "-" + NumberRange.Max + ".");
            }

            return new ComparisonItem(ComparisonTask.PutInOrder, Representation.Numbers, sorted);
        }

        public override bool IsCorrect(Answer answer)
        {
            if (Task == ComparisonTask.ChooseSign)
            {
                return answer.Sign.HasValue && answer.Sign.Value == ExpectedSign;
            }

            return answer.Order != null && answer.Order.SequenceEqual(_numbers);
        }

        /// <summary>
        /// A sign item: the two quantities side by side (told with the relation for numbers, with "here" and "there" for
        /// groups). An ordering item: the numbers in order.
        /// </summary>
        public override Explanation Explain()
        {
            if (Task == ComparisonTask.PutInOrder)
            {
                return Explanation.OrderedSequence(OrderPhrase(_numbers.Length), _numbers);
            }

            return Explanation.SideBySide(SignPhrase(), _numbers[0], _numbers[1], Representation);
        }

        /// <summary>
        /// Builds the item a key names; false for a key of another form, with a number outside 0-20, written with a leading
        /// zero, or (an ordering key) not three to five different numbers in ascending order.
        /// </summary>
        public static bool TryParse(ItemKey key, out ComparisonItem item)
        {
            item = null;
            string text = key.ToString();
            if (text.StartsWith(SignPrefix, StringComparison.Ordinal))
            {
                string body = text.Substring(SignPrefix.Length);
                Representation representation = Representation.Numbers;
                if (body.StartsWith(GroupMark, StringComparison.Ordinal))
                {
                    representation = Representation.Groups;
                    body = body.Substring(GroupMark.Length);
                }

                string[] parts = body.Split(SignSeparator);
                if (parts.Length != 2 || !ItemKey.TryReadNumber(parts[0], out int left) || !ItemKey.TryReadNumber(parts[1], out int right))
                {
                    return false;
                }

                item = ChooseSign(representation, left, right);
                return true;
            }

            if (text.StartsWith(OrderPrefix, StringComparison.Ordinal))
            {
                string[] parts = text.Substring(OrderPrefix.Length).Split(OrderSeparator);
                if (parts.Length < MinOrderCount || parts.Length > MaxOrderCount)
                {
                    return false;
                }

                var numbers = new List<int>();
                foreach (string part in parts)
                {
                    if (!ItemKey.TryReadNumber(part, out int number) || (numbers.Count > 0 && number <= numbers[numbers.Count - 1]))
                    {
                        return false;
                    }

                    numbers.Add(number);
                }

                item = PutInOrder(numbers);
                return true;
            }

            return false;
        }

        private PhraseKey SignPhrase()
        {
            if (Representation == Representation.Groups)
            {
                return PhraseKey.ExplainGroups;
            }

            switch (ExpectedSign)
            {
                case Sign.Less: return PhraseKey.ExplainLess;
                case Sign.Greater: return PhraseKey.ExplainMore;
                default: return PhraseKey.ExplainSame;
            }
        }

        private static PhraseKey OrderPhrase(int count)
        {
            switch (count)
            {
                case 3: return PhraseKey.ExplainOrder3;
                case 4: return PhraseKey.ExplainOrder4;
                default: return PhraseKey.ExplainOrder5;
            }
        }

        private string Format()
        {
            if (Task == ComparisonTask.PutInOrder)
            {
                return OrderPrefix + string.Join(OrderSeparator.ToString(), _numbers.Select(n => n.ToString(CultureInfo.InvariantCulture)));
            }

            return SignPrefix + (Representation == Representation.Groups ? GroupMark : string.Empty)
                + _numbers[0].ToString(CultureInfo.InvariantCulture) + SignSeparator + _numbers[1].ToString(CultureInfo.InvariantCulture);
        }
    }
}
