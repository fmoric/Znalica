using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    /// <summary>Answer records for tests, with the fields a test does not care about filled in.</summary>
    public static class TestRecords
    {
        public static AnswerRecord Make(
            int sessionNumber = 1,
            string itemKey = "A:8+5=?",
            string answer = "13",
            bool isCorrect = true,
            AttemptKind attempt = AttemptKind.First,
            string roundId = "round-1",
            Activity activity = Activity.Arithmetic,
            string answeredAt = "2026-11-17T10:30:00.0000000+01:00")
        {
            return new AnswerRecord(
                activity, itemKey, answer, isCorrect, attempt, 1234, answeredAt, sessionNumber, roundId);
        }
    }
}
