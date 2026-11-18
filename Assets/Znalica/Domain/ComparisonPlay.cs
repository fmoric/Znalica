using System.Collections.Generic;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>
    /// The child's side of one Veće, manje, jednako round. The screen draws this and calls its methods; it holds no rules.
    /// <list type="bullet">
    /// <item>A sign item is answered by <see cref="ChooseSign"/>: the click is the answer.</item>
    /// <item>An ordering item is answered by <see cref="PickNumber"/>, one click per number in order, smallest first. The
    /// last click gives the answer; <see cref="UndoPick"/> takes the last click back before that. A number clicked twice,
    /// or not in the item, is ignored.</item>
    /// <item>A sign click on an ordering item, and a number click on a sign item, do nothing.</item>
    /// <item>After a second wrong answer every click does nothing: only <see cref="Continue"/> goes on.</item>
    /// <item>The first click of the next answer, or an undo, clears the praise; an answer replaces it.</item>
    /// </list>
    /// </summary>
    public sealed class ComparisonPlay
    {
        private readonly RoundPlay _play;
        private readonly List<int> _picked = new List<int>();

        public ComparisonPlay(RoundSession session, IClock clock)
        {
            _play = new RoundPlay(session, clock);
        }

        /// <summary>The item to draw, or null once the round has ended.</summary>
        public ComparisonItem CurrentItem => (ComparisonItem)_play.CurrentItem;

        /// <summary>The numbers clicked so far for an ordering item, in the order clicked.</summary>
        public IReadOnlyList<int> Picked => _picked;

        public Feedback Feedback => _play.Feedback;

        public bool IsEnded => _play.IsEnded;

        /// <summary>True after a second wrong answer to an item: only going on is left, and the screen shows the Dalje button.</summary>
        public bool AwaitsContinue => _play.AwaitsContinue;

        public IReadOnlyDictionary<string, string> SummaryArgs => _play.SummaryArgs;

        /// <summary>Gives the sign as the answer. Null if the click does nothing.</summary>
        public Feedback ChooseSign(Sign sign)
        {
            if (!Accepts(ComparisonTask.ChooseSign))
            {
                return null;
            }

            return _play.Submit(Answer.FromSign(sign));
        }

        /// <summary>
        /// Adds the number to the order. Returns the feedback if this was the last number and so the answer; otherwise null.
        /// If the answer cannot be saved the exception leaves the clicks as they were before this one.
        /// </summary>
        public Feedback PickNumber(int number)
        {
            if (!Accepts(ComparisonTask.PutInOrder)
                || !CurrentItem.Numbers.Contains(number)
                || _picked.Contains(number))
            {
                return null;
            }

            _play.ClearPraise();
            if (_picked.Count + 1 < CurrentItem.Numbers.Count)
            {
                _picked.Add(number);
                return null;
            }

            Feedback feedback = _play.Submit(Answer.FromOrder(_picked.Concat(new[] { number })));
            _picked.Clear();
            return feedback;
        }

        /// <summary>Takes back the last number clicked; does nothing when none is.</summary>
        public void UndoPick()
        {
            if (!Accepts(ComparisonTask.PutInOrder))
            {
                return;
            }

            _play.ClearPraise();
            if (_picked.Count > 0)
            {
                _picked.RemoveAt(_picked.Count - 1);
            }
        }

        /// <summary>Goes on to the next item after a second wrong answer; does nothing at any other time.</summary>
        public void Continue()
        {
            _play.Continue();
        }

        private bool Accepts(ComparisonTask task)
        {
            return !IsEnded && !AwaitsContinue && CurrentItem.Task == task;
        }
    }
}
