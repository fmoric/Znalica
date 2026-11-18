using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>What the child is shown and told after one answer: the phrase to show and speak, with its arguments.</summary>
    public sealed class Feedback
    {
        private static readonly IReadOnlyDictionary<string, string> NoArgs = new Dictionary<string, string>();

        private Feedback(FeedbackKind kind, PhraseKey phrase, IReadOnlyDictionary<string, string> args, Explanation explanation)
        {
            Kind = kind;
            Phrase = phrase;
            Args = args;
            Explanation = explanation;
        }

        public FeedbackKind Kind { get; }

        public PhraseKey Phrase { get; }

        /// <summary>The arguments for the phrase's text and voice slots; empty for praise.</summary>
        public IReadOnlyDictionary<string, string> Args { get; }

        /// <summary>The picture to draw for an explanation; null for praise.</summary>
        public Explanation Explanation { get; }

        public static Feedback Praise(PhraseKey phrase)
        {
            return new Feedback(FeedbackKind.Praise, phrase, NoArgs, null);
        }

        public static Feedback Explain(Explanation explanation)
        {
            return new Feedback(FeedbackKind.Explanation, explanation.Phrase, explanation.PhraseArgs, explanation);
        }
    }
}
