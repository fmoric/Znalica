using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// One school-like check as the log tells it: when it was taken, its answers, and, only if it was finished, its score.
    /// A check whose closing line is missing (the app was closed, or the parent went back) is not finished and has no score.
    /// </summary>
    public sealed class CheckResult
    {
        public CheckResult(string checkId, DateTimeOffset date, IReadOnlyList<CheckLine> answers, CheckLine closing)
        {
            CheckId = checkId;
            Date = date;
            Answers = answers;
            Closing = closing;
        }

        public string CheckId { get; }

        /// <summary>When the first answer was given (the closing line's time for a log with only that line).</summary>
        public DateTimeOffset Date { get; }

        /// <summary>The answers in the order they were recorded.</summary>
        public IReadOnlyList<CheckLine> Answers { get; }

        private CheckLine Closing { get; }

        public bool IsFinished => Closing != null;

        /// <summary>The number of right answers; null while the check is not finished.</summary>
        public int? Score => Closing?.Score;

        /// <summary>The number of items the check held; null while the check is not finished.</summary>
        public int? ItemCount => Closing?.ItemCount;
    }
}
