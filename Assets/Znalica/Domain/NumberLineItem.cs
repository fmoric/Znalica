using System;
using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// An item of Brojevna crta: place a number on the 0-20 line (<c>L:Place:14</c>), find the number before or after
    /// one (<c>L:Before:14</c>, <c>L:After:14</c>), or find where a jump from a start by a signed step lands
    /// (<c>L:Jump:6+3</c>, <c>L:Jump:9-3</c>). Every number, the answer included, lies in 0-20, and a jump is 1 to
    /// <see cref="MaxJump"/> long.
    /// </summary>
    public sealed class NumberLineItem : Item
    {
        /// <summary>The longest jump, forwards or back.</summary>
        public const int MaxJump = 10;

        private const string KeyPrefix = "L:";
        private const char KeySeparator = ':';
        private const char Forwards = '+';
        private const char Backwards = '-';

        private NumberLineItem(NumberLineTask task, int target, int step)
        {
            Task = task;
            Target = target;
            Step = step;
            Key = ItemKey.Of(Format());
            Difficulty = Math.Max(target, Solution) <= 10 ? Difficulty.WithinTen : Difficulty.BeyondTenWithoutCrossing;
        }

        public NumberLineTask Task { get; }

        /// <summary>The number the task names: the one to place, the one to find the neighbour of, the start of a jump.</summary>
        public int Target { get; }

        /// <summary>The signed size of a jump; 0 for the other tasks.</summary>
        public int Step { get; }

        public override ItemKey Key { get; }

        public override Activity Activity => Activity.NumberLine;

        public override Difficulty Difficulty { get; }

        /// <summary>The number the child has to give: where it lies, its neighbour, or where the jump lands.</summary>
        public int Solution
        {
            get
            {
                switch (Task)
                {
                    case NumberLineTask.Before: return Target - 1;
                    case NumberLineTask.After: return Target + 1;
                    case NumberLineTask.Jump: return Target + Step;
                    default: return Target;
                }
            }
        }

        /// <summary>
        /// Items that ask about the same two numbers are one fact, so Q4's "any item of the fact in box 3" counts a pair as
        /// known after one right form: <c>Before:14</c> and <c>After:13</c> (the neighbours 13 and 14), and a jump with the
        /// one that undoes it (<c>6+3</c> and <c>9-3</c>, between 6 and 9). Placing a number is its own fact.
        /// </summary>
        public override string FactId
        {
            get
            {
                if (Task == NumberLineTask.Place)
                {
                    return base.FactId;
                }

                string kind = Task == NumberLineTask.Jump ? "J" : "N";
                return kind + Text(Math.Min(Target, Solution)) + "," + Text(Math.Max(Target, Solution));
            }
        }

        /// <exception cref="ArgumentException">A number outside 0-20.</exception>
        public static NumberLineItem Place(int number)
        {
            return Create(NumberLineTask.Place, number, 0);
        }

        /// <exception cref="ArgumentException">A number with no number before it in 0-20 (0, or outside the range).</exception>
        public static NumberLineItem Before(int number)
        {
            return Create(NumberLineTask.Before, number, 0);
        }

        /// <exception cref="ArgumentException">A number with no number after it in 0-20 (20, or outside the range).</exception>
        public static NumberLineItem After(int number)
        {
            return Create(NumberLineTask.After, number, 0);
        }

        /// <param name="start">Where the jump starts.</param>
        /// <param name="step">Signed: positive jumps on towards 20, negative back towards 0.</param>
        /// <exception cref="ArgumentException">A start or a landing outside 0-20, or a step that is 0 or longer than <see cref="MaxJump"/>.</exception>
        public static NumberLineItem Jump(int start, int step)
        {
            return Create(NumberLineTask.Jump, start, step);
        }

        public override bool IsCorrect(Answer answer)
        {
            return answer.Number.HasValue && answer.Number.Value == Solution;
        }

        /// <summary>
        /// A jump: the same jump on the line as an addition or subtraction shows. Placing, before and after: the right
        /// place on the line (the child's own click is the screen's to mark; the item does not know it).
        /// </summary>
        public override Explanation Explain()
        {
            switch (Task)
            {
                case NumberLineTask.Before:
                    return Explanation.PositionOnLine(PhraseKey.ExplainBefore, Solution, Target);
                case NumberLineTask.After:
                    return Explanation.PositionOnLine(PhraseKey.ExplainAfter, Solution, Target);
                case NumberLineTask.Jump:
                    return new Explanation(
                        ExplanationKind.NumberLineJump,
                        Step > 0 ? PhraseKey.ExplainJump : PhraseKey.ExplainJumpBack,
                        Target,
                        Step,
                        Solution,
                        Step < 0);
                default:
                    return Explanation.PositionOnLine(PhraseKey.ExplainPlace, Target, Target);
            }
        }

        /// <summary>
        /// Builds the item a key names; false for a key of another form, with a number outside 0-20 or written another
        /// way, for <c>L:Before:0</c> and <c>L:After:20</c> (no neighbour in range), and for a jump that lands outside
        /// 0-20 (<c>L:Jump:18+5</c>) or is 0 or longer than <see cref="MaxJump"/> (<c>L:Jump:6+11</c>).
        /// </summary>
        public static bool TryParse(ItemKey key, out NumberLineItem item)
        {
            item = null;
            string[] parts = key.ToString().Split(KeySeparator);
            // The task's name must be written as it is: Enum.TryParse alone also takes a number ("1") for a task.
            if (parts.Length != 3 || parts[0] + KeySeparator != KeyPrefix
                || !Enum.TryParse(parts[1], false, out NumberLineTask task) || task.ToString() != parts[1])
            {
                return false;
            }

            int target;
            int step = 0;
            if (task == NumberLineTask.Jump)
            {
                int sign = parts[2].IndexOfAny(new[] { Forwards, Backwards }, 1);
                if (sign < 0 || !ItemKey.TryReadNumber(parts[2].Substring(0, sign), out target)
                    || !ItemKey.TryReadNumber(parts[2].Substring(sign + 1), out int length))
                {
                    return false;
                }

                step = parts[2][sign] == Forwards ? length : -length;
            }
            else if (!ItemKey.TryReadNumber(parts[2], out target))
            {
                return false;
            }

            if (!Exists(task, target, step))
            {
                return false;
            }

            item = new NumberLineItem(task, target, step);
            return true;
        }

        private static NumberLineItem Create(NumberLineTask task, int target, int step)
        {
            if (!Exists(task, target, step))
            {
                throw new ArgumentException("There is no " + task + " item with the number " + target + " and the step " + step + ".");
            }

            return new NumberLineItem(task, target, step);
        }

        /// <summary>True when the number and the answer both lie in 0-20 and a jump has a length it may have.</summary>
        private static bool Exists(NumberLineTask task, int target, int step)
        {
            if (task == NumberLineTask.Jump && (step == 0 || Math.Abs(step) > MaxJump))
            {
                return false;
            }

            int solution = task == NumberLineTask.Before ? target - 1 : task == NumberLineTask.After ? target + 1 : target + step;
            return NumberRange.Contains(target) && NumberRange.Contains(solution);
        }

        private string Format()
        {
            string body = Task == NumberLineTask.Jump
                ? Text(Target) + (Step > 0 ? Forwards : Backwards) + Text(Math.Abs(Step))
                : Text(Target);
            return KeyPrefix + Task + KeySeparator + body;
        }

        private static string Text(int number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
