using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Znalica.Domain
{
    public enum ExplanationKind
    {
        /// <summary>Jumps on the 0-20 line: 8, then 5 more, 13. Later steps add the kinds of the other activities.</summary>
        NumberLineJump,

        /// <summary>Both quantities of a comparison side by side, in ten-frames for groups.</summary>
        SideBySide,

        /// <summary>The numbers of an ordering item in order on the line.</summary>
        OrderedSequence,

        /// <summary>The right place for a number on the 0-20 line (placing, and the number before or after one).</summary>
        PositionOnLine,
    }

    /// <summary>
    /// What a wrong answer is shown and told: the kind of picture, the numbers it needs, and the phrase that speaks it.
    /// Every wrong answer has one (there is no "wrong" without a reason). The numbers a kind uses are the properties
    /// named in its <see cref="ExplanationKind"/> value: the others are empty or zero.
    /// </summary>
    public sealed class Explanation
    {
        private static readonly IReadOnlyList<int> NoNumbers = new int[0];

        // Slot names of an ordering phrase, in the order of the numbers.
        private static readonly string[] OrderSlots = { "First", "Second", "Third", "Fourth", "Fifth" };

        private Explanation(
            ExplanationKind kind,
            PhraseKey phrase,
            int start,
            int change,
            int end,
            bool backwards,
            int left,
            int right,
            Representation representation,
            IReadOnlyList<int> numbers,
            int target,
            int given)
        {
            Kind = kind;
            Phrase = phrase;
            Start = start;
            Change = change;
            End = end;
            Backwards = backwards;
            Left = left;
            Right = right;
            Representation = representation;
            Numbers = numbers;
            Target = target;
            Given = given;
        }

        /// <summary>A jump on the number line.</summary>
        public Explanation(ExplanationKind kind, PhraseKey phrase, int start, int change, int end, bool backwards)
            : this(kind, phrase, start, change, end, backwards, 0, 0, Representation.Numbers, NoNumbers, 0, 0)
        {
        }

        public ExplanationKind Kind { get; }

        /// <summary>The phrase that speaks the explanation; its slots are filled from <see cref="PhraseArgs"/>.</summary>
        public PhraseKey Phrase { get; }

        /// <summary>Where the jump starts.</summary>
        public int Start { get; }

        /// <summary>The size of the jump, negative for a jump back.</summary>
        public int Change { get; }

        /// <summary>Where the jump lands.</summary>
        public int End { get; }

        /// <summary>
        /// True for a jump back (a subtraction). Kept apart from the sign of <see cref="Change"/>, because taking 0 away
        /// is still a jump back and has no sign to show it.
        /// </summary>
        public bool Backwards { get; }

        /// <summary>The quantity on the left of a <see cref="ExplanationKind.SideBySide"/> explanation.</summary>
        public int Left { get; }

        /// <summary>The quantity on the right of a <see cref="ExplanationKind.SideBySide"/> explanation.</summary>
        public int Right { get; }

        /// <summary>Whether a <see cref="ExplanationKind.SideBySide"/> explanation shows numbers or groups of objects.</summary>
        public Representation Representation { get; }

        /// <summary>The numbers of an <see cref="ExplanationKind.OrderedSequence"/>, smallest first.</summary>
        public IReadOnlyList<int> Numbers { get; }

        /// <summary>The number the child should have found, marked at its place on a <see cref="ExplanationKind.PositionOnLine"/> picture.</summary>
        public int Target { get; }

        /// <summary>The number the task named, which the answer was found from: the same as <see cref="Target"/> when placing.</summary>
        public int Given { get; }

        public static Explanation SideBySide(PhraseKey phrase, int left, int right, Representation representation)
        {
            return new Explanation(ExplanationKind.SideBySide, phrase, 0, 0, 0, false, left, right, representation, NoNumbers, 0, 0);
        }

        public static Explanation OrderedSequence(PhraseKey phrase, IEnumerable<int> numbers)
        {
            return new Explanation(
                ExplanationKind.OrderedSequence, phrase, 0, 0, 0, false, 0, 0, Representation.Numbers, numbers.ToArray(), 0, 0);
        }

        public static Explanation PositionOnLine(PhraseKey phrase, int target, int given)
        {
            return new Explanation(
                ExplanationKind.PositionOnLine, phrase, 0, 0, 0, false, 0, 0, Representation.Numbers, NoNumbers, target, given);
        }

        /// <summary>
        /// The arguments for the phrase's text and voice slots. A spoken number is a bare numeral, so
        /// <c>Change</c> is the size of the jump without its sign.
        /// </summary>
        public IReadOnlyDictionary<string, string> PhraseArgs
        {
            get
            {
                switch (Kind)
                {
                    case ExplanationKind.SideBySide:
                        return new Dictionary<string, string>
                        {
                            { "Left", Text(Left) },
                            { "Right", Text(Right) },
                        };
                    case ExplanationKind.PositionOnLine:
                        return new Dictionary<string, string>
                        {
                            { "Target", Text(Target) },
                            { "Given", Text(Given) },
                        };
                    case ExplanationKind.OrderedSequence:
                        var args = new Dictionary<string, string>();
                        for (int i = 0; i < Numbers.Count && i < OrderSlots.Length; i++)
                        {
                            args[OrderSlots[i]] = Text(Numbers[i]);
                        }

                        return args;
                    default:
                        return new Dictionary<string, string>
                        {
                            { "Start", Text(Start) },
                            { "Change", Text(Math.Abs(Change)) },
                            { "End", Text(End) },
                        };
                }
            }
        }

        private static string Text(int number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
