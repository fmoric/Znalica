namespace Znalica.Domain
{
    /// <summary>What one answer led to. A right answer carries praise and no explanation; a wrong one the reverse.</summary>
    public sealed class AttemptOutcome
    {
        public AttemptOutcome(bool isCorrect, Explanation explanation, PhraseKey? praisePhrase)
        {
            IsCorrect = isCorrect;
            Explanation = explanation;
            PraisePhrase = praisePhrase;
        }

        public bool IsCorrect { get; }

        /// <summary>Set for every wrong answer, null for a right one.</summary>
        public Explanation Explanation { get; }

        /// <summary>Set for a right answer, null for a wrong one. Praise plays over the next item; nothing waits for it.</summary>
        public PhraseKey? PraisePhrase { get; }
    }
}
