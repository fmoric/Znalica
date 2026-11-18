using System.Collections.Generic;

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
    /// The response time is <see cref="RoundPlay"/>'s.
    /// </summary>
    public sealed class ArithmeticPlay
    {
        private readonly RoundPlay _play;
        private AnswerEntry _entry = new AnswerEntry();

        public ArithmeticPlay(RoundSession session, IClock clock)
        {
            _play = new RoundPlay(session, clock);
        }

        /// <summary>The item to draw, or null once the round has ended.</summary>
        public Item CurrentItem => _play.CurrentItem;

        /// <summary>The number typed so far, or null while the box is empty.</summary>
        public int? Entered => _entry.Value;

        /// <summary>The feedback to keep on screen: praise or an explanation; null at the start and after Continue.</summary>
        public Feedback Feedback => _play.Feedback;

        public bool IsEnded => _play.IsEnded;

        /// <summary>True after a second wrong answer to an item: only going on is left, and the screen shows the Dalje button.</summary>
        public bool AwaitsContinue => _play.AwaitsContinue;

        /// <summary>The round-end screen's numbers, for the <c>Right</c> and <c>Total</c> slots of its phrase.</summary>
        public IReadOnlyDictionary<string, string> SummaryArgs => _play.SummaryArgs;

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
                    _play.Continue();
                }

                return null;
            }

            if (key == AnswerKey.Continue)
            {
                return null;
            }

            if (key != AnswerKey.Confirm)
            {
                _play.ClearPraise();
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

            Feedback feedback = _play.Submit(Answer.FromNumber(_entry.Value.Value));
            _entry = new AnswerEntry();
            return feedback;
        }
    }
}
