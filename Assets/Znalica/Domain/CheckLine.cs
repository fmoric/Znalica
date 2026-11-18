using System;

namespace Znalica.Domain
{
    /// <summary>
    /// One line of a profile's check log: the answer to one item of a check, or the closing line that says the check
    /// was finished. Reading the lines back in groups by <see cref="CheckId"/> gives the <see cref="CheckResult"/>s
    /// (<see cref="CheckHistory"/>).
    /// </summary>
    public sealed class CheckLine
    {
        private CheckLine()
        {
        }

        public string CheckId { get; private set; }

        /// <summary>True for the line that closes a finished check; false for an answer.</summary>
        public bool IsClosing { get; private set; }

        /// <summary>Answer line: the <see cref="ItemKey"/> as text.</summary>
        public string ItemKey { get; private set; }

        /// <summary>Answer line: the answer given, canonical (<c>13</c>, <c>Less</c>); empty when the item was left unanswered.</summary>
        public string Answer { get; private set; }

        public bool IsCorrect { get; private set; }

        /// <summary>Answer line: ISO 8601 with offset.</summary>
        public string AnsweredAt { get; private set; }

        /// <summary>Closing line: ISO 8601 with offset.</summary>
        public string CompletedAt { get; private set; }

        /// <summary>Closing line: how many answers were right.</summary>
        public int Score { get; private set; }

        /// <summary>Closing line: how many items the check held.</summary>
        public int ItemCount { get; private set; }

        public static CheckLine ForAnswer(string checkId, string itemKey, string answer, bool isCorrect, string answeredAt)
        {
            return new CheckLine
            {
                CheckId = Required(checkId, nameof(checkId)),
                ItemKey = Required(itemKey, nameof(itemKey)),
                Answer = answer ?? string.Empty,
                IsCorrect = isCorrect,
                AnsweredAt = Required(answeredAt, nameof(answeredAt)),
            };
        }

        public static CheckLine ForClosing(string checkId, string completedAt, int score, int itemCount)
        {
            if (itemCount < 1 || score < 0 || score > itemCount)
            {
                throw new ArgumentException("A score of " + score + " is not possible for " + itemCount + " items.");
            }

            return new CheckLine
            {
                CheckId = Required(checkId, nameof(checkId)),
                IsClosing = true,
                CompletedAt = Required(completedAt, nameof(completedAt)),
                Score = score,
                ItemCount = itemCount,
            };
        }

        private static string Required(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("A check line needs a value for " + name + ".", name);
            }

            return value;
        }
    }
}
