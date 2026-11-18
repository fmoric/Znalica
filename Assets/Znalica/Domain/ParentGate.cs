using System;

namespace Znalica.Domain
{
    /// <summary>
    /// The parent gate: a multiplication of two factors 3 to 9 that an adult answers in seconds and a grade 1 pupil
    /// cannot (functional spec, business rules). A wrong answer draws a new question; three wrong in a row make the gate
    /// wait 30 seconds, during which it takes no answer. No password. The wait is counted over the injected clock and
    /// kept by this object, so leaving the gate and coming back does not skip it: the app keeps one gate for its whole run.
    /// </summary>
    public sealed class ParentGate
    {
        public const int FactorMin = 3;
        public const int FactorMax = 9;
        public const int TriesBeforeWait = 3;
        public const int WaitSeconds = 30;

        /// <summary>The highest answer: nine times nine.</summary>
        public const int MaxAnswer = FactorMax * FactorMax;

        private readonly IClock _clock;
        private readonly IRandom _random;
        private readonly AnswerEntry _entry = new AnswerEntry(MaxAnswer);
        private int _wrongInARow;
        private DateTimeOffset _waitEnds = DateTimeOffset.MinValue;

        public ParentGate(IClock clock, IRandom random)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Challenge = Next(random);
        }

        /// <summary>The question now asked.</summary>
        public GateChallenge Challenge { get; private set; }

        /// <summary>The number typed so far, or null while the box is empty.</summary>
        public int? Entered => _entry.Value;

        public bool IsWaiting => _clock.Now < _waitEnds;

        /// <summary>Whole seconds left of the wait, rounded up; 0 when not waiting.</summary>
        public int SecondsLeft
        {
            get
            {
                TimeSpan left = _waitEnds - _clock.Now;
                return left <= TimeSpan.Zero ? 0 : (int)Math.Ceiling(left.TotalSeconds);
            }
        }

        /// <summary>A question with both factors drawn from 3 to 9.</summary>
        public static GateChallenge Next(IRandom random)
        {
            int span = FactorMax - FactorMin + 1;
            return new GateChallenge(FactorMin + random.Next(span), FactorMin + random.Next(span));
        }

        public static bool Check(GateChallenge challenge, int answer)
        {
            return challenge.Answer == answer;
        }

        /// <summary>Applies a pad button or key: digits and Erase edit the box, Confirm gives the answer.</summary>
        public GateResult Press(AnswerKey key)
        {
            if (IsWaiting)
            {
                return GateResult.Locked;
            }

            if (key == AnswerKey.Erase)
            {
                _entry.Erase();
                return GateResult.Typing;
            }

            if (key == AnswerKey.Confirm)
            {
                return Confirm();
            }

            if (key >= AnswerKey.Digit0 && key <= AnswerKey.Digit9)
            {
                _entry.TryType(key - AnswerKey.Digit0);
            }

            return GateResult.Typing;
        }

        private GateResult Confirm()
        {
            if (!_entry.Value.HasValue)
            {
                return GateResult.Typing;
            }

            int answer = _entry.Value.Value;
            bool right = Check(Challenge, answer);
            _entry.Clear();
            Challenge = Next(_random);
            if (right)
            {
                _wrongInARow = 0;
                return GateResult.Passed;
            }

            _wrongInARow++;
            if (_wrongInARow < TriesBeforeWait)
            {
                return GateResult.Wrong;
            }

            _wrongInARow = 0;
            _waitEnds = _clock.Now + TimeSpan.FromSeconds(WaitSeconds);
            return GateResult.Locked;
        }
    }
}
