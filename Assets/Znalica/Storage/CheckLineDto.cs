using System;
using System.Globalization;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// One line of <c>Checks.jsonl</c>: an answer (<c>Line</c> is <c>Answer</c>) or the closing line of a finished check
    /// (<c>Line</c> is <c>Closing</c>). One object with the fields of both, public for JsonUtility; the fields a kind does not
    /// use are left empty.
    /// </summary>
    [Serializable]
    internal sealed class CheckLineDto
    {
        private const string AnswerLine = "Answer";
        private const string ClosingLine = "Closing";

        public string Line;
        public string CheckId;
        public string ItemKey;
        public string Answer;
        public bool IsCorrect;
        public string AnsweredAt;
        public string CompletedAt;
        public int Score;
        public int ItemCount;

        public static CheckLineDto From(CheckLine line)
        {
            return new CheckLineDto
            {
                Line = line.IsClosing ? ClosingLine : AnswerLine,
                CheckId = line.CheckId,
                ItemKey = line.ItemKey,
                Answer = line.Answer,
                IsCorrect = line.IsCorrect,
                AnsweredAt = line.AnsweredAt,
                CompletedAt = line.CompletedAt,
                Score = line.Score,
                ItemCount = line.ItemCount,
            };
        }

        /// <summary>The line, or false when a field is missing or not what its kind allows (a line a crash left half-written).</summary>
        public bool TryToLine(out CheckLine line)
        {
            line = null;
            if (string.IsNullOrEmpty(CheckId))
            {
                return false;
            }

            if (Line == AnswerLine)
            {
                if (string.IsNullOrEmpty(ItemKey) || !IsDate(AnsweredAt))
                {
                    return false;
                }

                line = CheckLine.ForAnswer(CheckId, ItemKey, Answer, IsCorrect, AnsweredAt);
                return true;
            }

            if (Line == ClosingLine && IsDate(CompletedAt) && ItemCount >= 1 && Score >= 0 && Score <= ItemCount)
            {
                line = CheckLine.ForClosing(CheckId, CompletedAt, Score, ItemCount);
                return true;
            }

            return false;
        }

        private static bool IsDate(string text)
        {
            return DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _);
        }
    }
}
