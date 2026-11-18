using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Znalica.Domain
{
    /// <summary>
    /// <c>First op Second = Result</c> with one of the three numbers left for the child to find: <c>8 + 5 = ?</c>,
    /// <c>8 + ? = 13</c>, <c>? + 5 = 13</c> and the three subtraction forms. Every number lies in 0-20.
    /// </summary>
    public sealed class ArithmeticItem : Item
    {
        private const string KeyPrefix = "A:";

        /// <summary>The mark of the missing number in a key (<c>A:8+?=13</c>), and in what is shown.</summary>
        public const string UnknownMark = "?";
        private const string Number = "(\\?|0|[1-9][0-9]?)";

        // The one definition of the key's form; Format below writes the same shape.
        private static readonly Regex KeyForm = new Regex(
            "^" + KeyPrefix + Number + "([+-])" + Number + "=" + Number + "\\z",
            RegexOptions.CultureInvariant);

        /// <exception cref="ArgumentException">A number outside 0-20, or a fact that is not true.</exception>
        public ArithmeticItem(int first, Operation operation, int second, int result, Unknown unknown)
        {
            if (!Enum.IsDefined(typeof(Unknown), unknown))
            {
                throw new ArgumentException("Unknown must name one of the three numbers.", nameof(unknown));
            }

            if (!NumberRange.Contains(first) || !NumberRange.Contains(second) || !NumberRange.Contains(result))
            {
                throw new ArgumentException("The numbers " + first + ", " + second + ", " + result + " are not all in "
                    + NumberRange.Min + "-" + NumberRange.Max + ".");
            }

            int stated = operation == Operation.Addition ? first + second : first - second;
            if (stated != result)
            {
                throw new ArgumentException("The fact " + first + " " + operation + " " + second + " is not " + result + ".");
            }

            First = first;
            Operation = operation;
            Second = second;
            Result = result;
            Unknown = unknown;
            Key = ItemKey.Of(Format());
            Difficulty = DifficultyOf(first, operation, second, result);
        }

        public int First { get; }

        public Operation Operation { get; }

        public int Second { get; }

        public int Result { get; }

        public Unknown Unknown { get; }

        public override ItemKey Key { get; }

        public override Activity Activity => Activity.Arithmetic;

        public override Difficulty Difficulty { get; }

        /// <summary>The number the child has to find.</summary>
        public int Solution
        {
            get
            {
                switch (Unknown)
                {
                    case Unknown.First: return First;
                    case Unknown.Second: return Second;
                    default: return Result;
                }
            }
        }

        public override bool IsCorrect(Answer answer)
        {
            return answer.Number.HasValue && answer.Number.Value == Solution;
        }

        /// <summary>The jump on the number line from the first number by the second: 8, then 5 more, 13.</summary>
        public override Explanation Explain()
        {
            bool addition = Operation == Operation.Addition;
            return new Explanation(
                ExplanationKind.NumberLineJump,
                addition ? PhraseKey.ExplainJump : PhraseKey.ExplainJumpBack,
                First,
                addition ? Second : -Second,
                Result,
                !addition);
        }

        /// <summary>
        /// Builds the item a key names; false for a key of another form, with a number outside 0-20 (the stated ones, or
        /// the missing one: <c>A:3-?=7</c> would need -4), or without exactly one missing number (<c>A:8+5=14</c>).
        /// </summary>
        public static bool TryParse(ItemKey key, out ArithmeticItem item)
        {
            item = null;
            Match match = KeyForm.Match(key.ToString());
            if (!match.Success)
            {
                return false;
            }

            Operation operation = match.Groups[2].Value == "+" ? Operation.Addition : Operation.Subtraction;
            int? first = ReadNumber(match.Groups[1].Value);
            int? second = ReadNumber(match.Groups[3].Value);
            int? result = ReadNumber(match.Groups[4].Value);
            int missing = (first.HasValue ? 0 : 1) + (second.HasValue ? 0 : 1) + (result.HasValue ? 0 : 1);
            if (missing != 1)
            {
                return false;
            }

            Unknown unknown;
            if (!result.HasValue)
            {
                unknown = Unknown.Result;
                result = operation == Operation.Addition ? first + second : first - second;
            }
            else if (!second.HasValue)
            {
                unknown = Unknown.Second;
                second = operation == Operation.Addition ? result - first : first - result;
            }
            else
            {
                unknown = Unknown.First;
                first = operation == Operation.Addition ? result - second : result + second;
            }

            if (!NumberRange.Contains(first.Value) || !NumberRange.Contains(second.Value) || !NumberRange.Contains(result.Value))
            {
                return false;
            }

            item = new ArithmeticItem(first.Value, operation, second.Value, result.Value, unknown);
            return true;
        }

        /// <summary>
        /// Within ten: every number is 10 or less. Crossing ten: the fact's two parts are both below 10 and the whole
        /// is above it (<c>8 + 5</c>, <c>13 - 6</c>, in every one of the four forms of the fact). Otherwise beyond ten
        /// without crossing, including facts that start from or land on 10 (<c>10 + 5</c>, <c>13 - 3</c>).
        /// </summary>
        private static Difficulty DifficultyOf(int first, Operation operation, int second, int result)
        {
            int whole = operation == Operation.Addition ? result : first;
            int largestPart = operation == Operation.Addition ? Math.Max(first, second) : Math.Max(second, result);
            if (whole <= 10)
            {
                return Difficulty.WithinTen;
            }

            return largestPart < 10 ? Difficulty.CrossingTen : Difficulty.BeyondTenWithoutCrossing;
        }

        private static int? ReadNumber(string text)
        {
            return text == UnknownMark ? (int?)null : int.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture);
        }

        private string Format()
        {
            return KeyPrefix + Text(First, Unknown.First) + (Operation == Operation.Addition ? "+" : "-")
                + Text(Second, Unknown.Second) + "=" + Text(Result, Unknown.Result);
        }

        private string Text(int number, Unknown position)
        {
            return Unknown == position ? UnknownMark : number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
