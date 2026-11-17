namespace Znalica.Domain
{
    /// <summary>
    /// One answer as the answer log keeps it. The item schedule, the progress view and the school-like check's
    /// comparison all read these records, so a field is added here only with its reader in mind.
    /// </summary>
    public sealed class AnswerRecord
    {
        public AnswerRecord(
            Activity activity,
            string itemKey,
            string answer,
            bool isCorrect,
            AttemptKind attempt,
            int responseMilliseconds,
            string answeredAt,
            int sessionNumber,
            string roundId)
        {
            Activity = activity;
            ItemKey = itemKey;
            Answer = answer;
            IsCorrect = isCorrect;
            Attempt = attempt;
            ResponseMilliseconds = responseMilliseconds;
            AnsweredAt = answeredAt;
            SessionNumber = sessionNumber;
            RoundId = roundId;
        }

        public Activity Activity { get; }

        /// <summary>The <see cref="Znalica.Domain.ItemKey"/> as text.</summary>
        public string ItemKey { get; }

        /// <summary>The answer given, canonical (<c>13</c>).</summary>
        public string Answer { get; }

        public bool IsCorrect { get; }

        public AttemptKind Attempt { get; }

        /// <summary>Time to answer; recorded, never shown to the child.</summary>
        public int ResponseMilliseconds { get; }

        /// <summary>ISO 8601 with offset (<c>DateTimeOffset.ToString("o")</c>).</summary>
        public string AnsweredAt { get; }

        /// <summary>The profile's session counter.</summary>
        public int SessionNumber { get; }

        public string RoundId { get; }

        /// <summary>Right first time: the first ask of the item in the round, answered right.</summary>
        public bool IsRightFirstTime => Attempt == AttemptKind.First && IsCorrect;
    }
}
