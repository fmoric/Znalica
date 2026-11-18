using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// The child's side of one Brojevna crta round. The screen draws this and calls its methods; it holds no rules.
    /// <list type="bullet">
    /// <item>A <see cref="NumberLineTask.Place"/> item is answered by <see cref="PlaceAt"/>: a click on the line, snapped to
    /// the nearest whole number. It is right only when that number is the one asked for, so a click nearer to the
    /// neighbour is wrong, and no click is "close enough". The pad and the keys do nothing for it.</item>
    /// <item>Before, After and Jump items are answered with the pad and the keys, as in arithmetic (the answer box,
    /// <see cref="Press"/>). A click on the line does nothing for them.</item>
    /// <item>After a second wrong answer only <see cref="AnswerKey.Continue"/> goes on: clicks, digits and U redu do nothing.</item>
    /// </list>
    /// The typed answer and the round are <see cref="ArithmeticPlay"/>'s, which serves every activity whose answer is a number.
    /// </summary>
    public sealed class NumberLinePlay
    {
        private readonly ArithmeticPlay _play;
        private int? _lastPlaced;

        public NumberLinePlay(RoundSession session, IClock clock)
        {
            _play = new ArithmeticPlay(session, clock);
        }

        /// <summary>The item to draw, or null once the round has ended.</summary>
        public NumberLineItem CurrentItem => (NumberLineItem)_play.CurrentItem;

        /// <summary>The number typed so far, or null while the box is empty.</summary>
        public int? Entered => _play.Entered;

        /// <summary>The number the last click on the line chose, for the explanation to show beside the right place; null before any.</summary>
        public int? LastPlaced => _lastPlaced;

        public Feedback Feedback => _play.Feedback;

        public bool IsEnded => _play.IsEnded;

        /// <summary>True after a second wrong answer to an item: only going on is left, and the screen shows the Dalje button.</summary>
        public bool AwaitsContinue => _play.AwaitsContinue;

        public IReadOnlyDictionary<string, string> SummaryArgs => _play.SummaryArgs;

        /// <summary>The whole number nearest to a position on the line, kept within 0-20: a click past an end chooses that end.</summary>
        public static int NearestNumber(double position)
        {
            double nearest = Math.Round(position, MidpointRounding.AwayFromZero);
            return (int)Math.Max(NumberRange.Min, Math.Min(NumberRange.Max, nearest));
        }

        /// <summary>
        /// Gives the click as the answer to a Place item. <paramref name="position"/> is where on the line the click fell, in
        /// numbers (13.4 is a little right of 13). Returns the feedback, or null if the click does nothing.
        /// </summary>
        public Feedback PlaceAt(double position)
        {
            if (IsEnded || double.IsNaN(position) || CurrentItem.Task != NumberLineTask.Place)
            {
                return null;
            }

            int number = NearestNumber(position);
            Feedback feedback = _play.Give(number);
            if (feedback != null)
            {
                _lastPlaced = number;
            }

            return feedback;
        }

        /// <summary>Does what the key means for a Before, After or Jump item (see <see cref="ArithmeticPlay.Press"/>); only Continue does anything for a Place item.</summary>
        public Feedback Press(AnswerKey key)
        {
            if (!IsEnded && CurrentItem.Task == NumberLineTask.Place && key != AnswerKey.Continue)
            {
                return null;
            }

            return _play.Press(key);
        }
    }
}
