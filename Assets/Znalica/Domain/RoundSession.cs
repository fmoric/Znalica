using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// One round, as a state machine the views render and call. Every answer is appended to the answer log and applied to
    /// the item schedule before <see cref="Submit"/> returns.
    /// <list type="bullet">
    /// <item>Right: the next item is current at once; nothing waits.</item>
    /// <item>Wrong on the first ask of an item: the explanation shows, the same item is asked again (Retry), and the item
    /// is also inserted to come back (Return) after three other items, or last if fewer than three other items remain: at most once per item.</item>
    /// <item>Wrong on a Retry: the explanation shows again and <see cref="Continue"/> goes on, so a child cannot get stuck.</item>
    /// </list>
    /// </summary>
    public sealed class RoundSession
    {
        /// <summary>How many other items are asked between a wrong item's first ask and its Return (fewer if fewer remain).</summary>
        public const int ReturnDistance = 3;

        private readonly IAnswerLog _log;
        private readonly ItemSchedule _schedule;
        private readonly IClock _clock;
        private readonly int _sessionNumber;
        private readonly string _roundId;
        private readonly List<Pending> _upcoming;

        private Item _current;
        private AttemptKind _currentAttempt;
        private bool _retryPending;

        public RoundSession(IReadOnlyList<Item> items, IAnswerLog log, ItemSchedule schedule, IClock clock, int sessionNumber, string roundId)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _log = log ?? throw new ArgumentNullException(nameof(log));
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _sessionNumber = sessionNumber;
            _roundId = roundId ?? throw new ArgumentNullException(nameof(roundId));
            PlannedCount = items.Count;
            _upcoming = new List<Pending>(items.Count);
            foreach (Item item in items)
            {
                _upcoming.Add(new Pending(item, AttemptKind.First));
            }

            MoveToNext();
        }

        public RoundState State { get; private set; }

        /// <summary>The item being asked, or being explained; null once the round has ended.</summary>
        public Item CurrentItem => _current;

        /// <summary>How the next <see cref="Submit"/> will be recorded; null when Submit would throw (no retry left, or the round has ended).</summary>
        public AttemptKind? NextAttempt
        {
            get
            {
                switch (State)
                {
                    case RoundState.AwaitingAnswer: return _currentAttempt;
                    case RoundState.ShowingExplanation: return _retryPending ? AttemptKind.Retry : (AttemptKind?)null;
                    default: return null;
                }
            }
        }

        /// <summary>In <see cref="RoundState.ShowingExplanation"/>: true if the item is asked again, false if only <see cref="Continue"/> goes on.</summary>
        public bool CanRetry => State == RoundState.ShowingExplanation && _retryPending;

        /// <summary>Items answered right on their first ask; Retry and Return never count.</summary>
        public int FirstTimeRightCount { get; private set; }

        /// <summary>The number of items the round was planned with (Return asks are not counted).</summary>
        public int PlannedCount { get; }

        public AttemptOutcome Submit(Answer answer, int responseMilliseconds)
        {
            if (State == RoundState.Ended)
            {
                throw new InvalidOperationException("The round has ended.");
            }

            if (State == RoundState.ShowingExplanation && !_retryPending)
            {
                throw new InvalidOperationException("The item is over; call Continue.");
            }

            if (responseMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(responseMilliseconds));
            }

            AttemptKind attempt = NextAttempt.Value;
            Item item = _current;
            bool correct = item.IsCorrect(answer);
            var record = new AnswerRecord(
                item.Activity,
                item.Key.ToString(),
                answer.Canonical(),
                correct,
                attempt,
                responseMilliseconds,
                _clock.Now.ToString("o"),
                _sessionNumber,
                _roundId);
            _log.Append(record);
            _schedule.Apply(record);

            if (correct)
            {
                if (attempt == AttemptKind.First)
                {
                    FirstTimeRightCount++;
                }

                MoveToNext();
                return new AttemptOutcome(true, null, PhraseKey.PraiseRight);
            }

            if (attempt == AttemptKind.First)
            {
                // Index 3 of the upcoming items: exactly three other items come before the Return.
                _upcoming.Insert(Math.Min(ReturnDistance, _upcoming.Count), new Pending(item, AttemptKind.Return));
            }

            _retryPending = attempt != AttemptKind.Retry;
            State = RoundState.ShowingExplanation;
            return new AttemptOutcome(false, item.Explain(), null);
        }

        /// <summary>Goes on after the second wrong answer to an item.</summary>
        public void Continue()
        {
            if (State != RoundState.ShowingExplanation || _retryPending)
            {
                throw new InvalidOperationException("Continue is for after a second wrong answer.");
            }

            MoveToNext();
        }

        private void MoveToNext()
        {
            _retryPending = false;
            if (_upcoming.Count == 0)
            {
                _current = null;
                State = RoundState.Ended;
                return;
            }

            _current = _upcoming[0].Item;
            _currentAttempt = _upcoming[0].Attempt;
            _upcoming.RemoveAt(0);
            State = RoundState.AwaitingAnswer;
        }

        private readonly struct Pending
        {
            public Pending(Item item, AttemptKind attempt)
            {
                Item = item;
                Attempt = attempt;
            }

            public Item Item { get; }

            public AttemptKind Attempt { get; }
        }
    }
}
