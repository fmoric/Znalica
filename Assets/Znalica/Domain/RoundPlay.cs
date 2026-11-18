using System;
using System.Collections.Generic;
using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// What every activity's play shares: the round, the feedback that is current, the response time and going on after a
    /// second wrong answer. <see cref="ArithmeticPlay"/> and <see cref="ComparisonPlay"/> add how their own answer is
    /// put together (digits typed, a sign chosen, numbers clicked).
    /// The response time of an answer runs from when its item was last put in front of the child (the round start, the
    /// previous answer, or Continue) to the answer.
    /// </summary>
    public sealed class RoundPlay
    {
        private readonly RoundSession _session;
        private readonly IClock _clock;
        private DateTimeOffset _shownAt;

        public RoundPlay(RoundSession session, IClock clock)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _shownAt = clock.Now;
        }

        /// <summary>The item to draw, or null once the round has ended.</summary>
        public Item CurrentItem => _session.CurrentItem;

        /// <summary>The feedback to keep on screen: praise or an explanation; null at the start and after Continue.</summary>
        public Feedback Feedback { get; private set; }

        public bool IsEnded => _session.State == RoundState.Ended;

        /// <summary>True after a second wrong answer to an item: only going on is left, and the screen shows the Dalje button.</summary>
        public bool AwaitsContinue => _session.State == RoundState.ShowingExplanation && !_session.CanRetry;

        /// <summary>The round-end screen's numbers, for the <c>Right</c> and <c>Total</c> slots of its phrase.</summary>
        public IReadOnlyDictionary<string, string> SummaryArgs => new Dictionary<string, string>
        {
            { "Right", _session.FirstTimeRightCount.ToString(CultureInfo.InvariantCulture) },
            { "Total", _session.PlannedCount.ToString(CultureInfo.InvariantCulture) },
        };

        /// <summary>
        /// Gives the answer to the round, and returns the feedback it produced (also kept in <see cref="Feedback"/>). If the
        /// round throws (the answer could not be saved) nothing here has moved.
        /// </summary>
        public Feedback Submit(Answer answer)
        {
            DateTimeOffset now = _clock.Now;
            double elapsed = Math.Max(0, (now - _shownAt).TotalMilliseconds);
            AttemptOutcome outcome = _session.Submit(answer, (int)Math.Min(elapsed, int.MaxValue));
            _shownAt = now;
            Feedback = outcome.IsCorrect ? Feedback.Praise(outcome.PraisePhrase.Value) : Feedback.Explain(outcome.Explanation);
            return Feedback;
        }

        /// <summary>Goes on to the next item after a second wrong answer; does nothing at any other time.</summary>
        public void Continue()
        {
            if (!AwaitsContinue)
            {
                return;
            }

            _session.Continue();
            Feedback = null;
            _shownAt = _clock.Now;
        }

        /// <summary>Takes praise off the screen once the child starts the next answer; an explanation stays.</summary>
        public void ClearPraise()
        {
            if (Feedback != null && Feedback.Kind == FeedbackKind.Praise)
            {
                Feedback = null;
            }
        }
    }
}
