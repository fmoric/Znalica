using System;
using System.Collections.Generic;
using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// The child's side of one arithmetic round: what the pad and the keys do to the answer box, when an answer is
    /// given, and what feedback is current. The screen draws this and calls <see cref="Press"/>; it holds no rules.
    /// <list type="bullet">
    /// <item>A digit goes into the box if <see cref="AnswerEntry"/> takes it; Erase removes the last one.</item>
    /// <item>Confirm with an empty box does nothing. Otherwise the answer goes to the round, the box is emptied, and the
    /// feedback is praise (the next item is already current) or the explanation (the same item is asked again).</item>
    /// <item>The first digit or Erase of the next answer clears the praise, so it does not stay beside the next item.</item>
    /// <item>After a second wrong answer the digits and Confirm do nothing: only <see cref="AnswerKey.Continue"/> goes on
    /// to the next item. A repeated click on the pad's U redu therefore cannot skip the explanation unseen.</item>
    /// </list>
    /// The response time of an answer runs from when its item was last put in front of the child (the round start,
    /// the previous answer, or Continue) to the Confirm.
    /// </summary>
    public sealed class ArithmeticPlay
    {
        private readonly RoundSession _session;
        private readonly IClock _clock;
        private AnswerEntry _entry = new AnswerEntry();
        private DateTimeOffset _shownAt;

        public ArithmeticPlay(RoundSession session, IClock clock)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _shownAt = clock.Now;
        }

        /// <summary>The item to draw, or null once the round has ended.</summary>
        public Item CurrentItem => _session.CurrentItem;

        /// <summary>The number typed so far, or null while the box is empty.</summary>
        public int? Entered => _entry.Value;

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

        /// <summary>Does what the key means. Returns the feedback this press produced, or null if it produced none.</summary>
        public Feedback Press(AnswerKey key)
        {
            if (IsEnded)
            {
                return null;
            }

            if (AwaitsContinue)
            {
                if (key == AnswerKey.Continue)
                {
                    _session.Continue();
                    Feedback = null;
                    _shownAt = _clock.Now;
                }

                return null;
            }

            if (key == AnswerKey.Continue)
            {
                return null;
            }

            if (key != AnswerKey.Confirm && Feedback != null && Feedback.Kind == FeedbackKind.Praise)
            {
                Feedback = null;
            }

            if (key >= AnswerKey.Digit0 && key <= AnswerKey.Digit9)
            {
                _entry.TryType(key - AnswerKey.Digit0);
                return null;
            }

            if (key == AnswerKey.Erase)
            {
                _entry.Erase();
                return null;
            }

            return Submit();
        }

        private Feedback Submit()
        {
            if (!_entry.Value.HasValue)
            {
                return null;
            }

            DateTimeOffset now = _clock.Now;
            double elapsed = Math.Max(0, (now - _shownAt).TotalMilliseconds);
            AttemptOutcome outcome = _session.Submit(Answer.FromNumber(_entry.Value.Value), (int)Math.Min(elapsed, int.MaxValue));
            _entry = new AnswerEntry();
            _shownAt = now;
            Feedback = outcome.IsCorrect ? Feedback.Praise(outcome.PraisePhrase.Value) : Feedback.Explain(outcome.Explanation);
            return Feedback;
        }
    }
}
