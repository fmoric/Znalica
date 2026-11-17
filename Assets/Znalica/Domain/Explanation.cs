using System;
using System.Collections.Generic;
using System.Globalization;

namespace Znalica.Domain
{
    public enum ExplanationKind
    {
        /// <summary>Jumps on the 0-20 line: 8, then 5 more, 13. Later steps add the kinds of the other activities.</summary>
        NumberLineJump,
    }

    /// <summary>
    /// What a wrong answer is shown and told: the kind of picture, the numbers it needs, and the phrase that speaks it.
    /// Every wrong answer has one (there is no "wrong" without a reason).
    /// </summary>
    public sealed class Explanation
    {
        public Explanation(ExplanationKind kind, PhraseKey phrase, int start, int change, int end)
        {
            Kind = kind;
            Phrase = phrase;
            Start = start;
            Change = change;
            End = end;
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
        /// The arguments for the phrase's text and voice slots. A spoken number is a bare numeral, so
        /// <c>Change</c> is the size of the jump without its sign.
        /// </summary>
        public IReadOnlyDictionary<string, string> PhraseArgs => new Dictionary<string, string>
        {
            { "Start", Start.ToString(CultureInfo.InvariantCulture) },
            { "Change", Math.Abs(Change).ToString(CultureInfo.InvariantCulture) },
            { "End", End.ToString(CultureInfo.InvariantCulture) },
        };
    }
}
