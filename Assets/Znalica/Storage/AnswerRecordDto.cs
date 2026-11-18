using System;
using System.Globalization;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// One line of <c>Answers.jsonl</c>. Public fields for JsonUtility; enums as their names. The domain's
    /// <see cref="AnswerRecord"/> keeps no engine reference, so this maps to it.
    /// </summary>
    [Serializable]
    internal sealed class AnswerRecordDto
    {
        public string Activity;
        public string ItemKey;
        public string Answer;
        public bool IsCorrect;
        public string Attempt;
        public int ResponseMilliseconds;
        public string AnsweredAt;
        public int SessionNumber;
        public string RoundId;

        public static AnswerRecordDto From(AnswerRecord record)
        {
            return new AnswerRecordDto
            {
                Activity = record.Activity.ToString(),
                ItemKey = record.ItemKey,
                Answer = record.Answer,
                IsCorrect = record.IsCorrect,
                Attempt = record.Attempt.ToString(),
                ResponseMilliseconds = record.ResponseMilliseconds,
                AnsweredAt = record.AnsweredAt,
                SessionNumber = record.SessionNumber,
                RoundId = record.RoundId,
            };
        }

        /// <summary>The record, or false when a field is missing or not what its type allows (a line a crash left half-written).</summary>
        public bool TryToRecord(out AnswerRecord record)
        {
            record = null;
            if (!Enum.TryParse(Activity, out Activity activity) || !Enum.IsDefined(typeof(Activity), activity)
                || !Enum.TryParse(Attempt, out AttemptKind attempt) || !Enum.IsDefined(typeof(AttemptKind), attempt)
                || string.IsNullOrEmpty(ItemKey)
                || string.IsNullOrEmpty(Answer)
                || string.IsNullOrEmpty(RoundId)
                || !DateTimeOffset.TryParse(AnsweredAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
            {
                return false;
            }

            record = new AnswerRecord(activity, ItemKey, Answer, IsCorrect, attempt, ResponseMilliseconds, AnsweredAt, SessionNumber, RoundId);
            return true;
        }
    }
}
