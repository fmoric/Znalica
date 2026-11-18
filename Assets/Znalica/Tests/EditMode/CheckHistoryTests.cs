using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class CheckHistoryTests
    {
        private static CheckLine Answer(string checkId, string key, bool correct, string at)
        {
            return CheckLine.ForAnswer(checkId, key, correct ? "13" : "9", correct, at);
        }

        [Test]
        public void From_NoLines_IsEmpty()
        {
            Assert.IsEmpty(CheckHistory.From(new List<CheckLine>()));
        }

        [Test]
        public void From_ChecksComeInTheOrderTheyWereTaken_WithTheDateOfTheirFirstAnswer()
        {
            CheckLine[] lines =
            {
                Answer("a", "L:Before:14", true, "2026-11-10T10:00:00.0000000+01:00"),
                Answer("a", "L:After:12", false, "2026-11-10T10:00:01.0000000+01:00"),
                CheckLine.ForClosing("a", "2026-11-10T10:05:00.0000000+01:00", 1, 2),
                Answer("b", "L:Before:7", true, "2026-11-12T09:00:00.0000000+01:00"),
                CheckLine.ForClosing("b", "2026-11-12T09:04:00.0000000+01:00", 1, 1),
                Answer("c", "L:Before:9", true, "2026-11-17T18:30:00.0000000+01:00"),
            };

            IReadOnlyList<CheckResult> results = CheckHistory.From(lines);

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, results.Select(result => result.CheckId).ToArray());
            Assert.AreEqual(new System.DateTimeOffset(2026, 11, 10, 10, 0, 0, System.TimeSpan.FromHours(1)), results[0].Date);
            Assert.AreEqual(new System.DateTimeOffset(2026, 11, 17, 18, 30, 0, System.TimeSpan.FromHours(1)), results[2].Date);
        }

        [Test]
        public void From_AFinishedCheckHasItsScoreAndItemCount_AnUnfinishedOneHasNeither()
        {
            CheckLine[] lines =
            {
                Answer("done", "L:Before:14", true, "2026-11-10T10:00:00.0000000+01:00"),
                CheckLine.ForClosing("done", "2026-11-10T10:05:00.0000000+01:00", 17, 20),
                Answer("left", "L:Before:7", true, "2026-11-12T09:00:00.0000000+01:00"),
            };

            IReadOnlyList<CheckResult> results = CheckHistory.From(lines);

            Assert.IsTrue(results[0].IsFinished);
            Assert.AreEqual(17, results[0].Score);
            Assert.AreEqual(20, results[0].ItemCount);
            Assert.IsFalse(results[1].IsFinished);
            Assert.IsNull(results[1].Score);
            Assert.IsNull(results[1].ItemCount);
            Assert.AreEqual(1, results[1].Answers.Count);
        }

        [Test]
        public void From_LinesOfTwoChecksMixedTogether_AreGroupedById()
        {
            CheckLine[] lines =
            {
                Answer("a", "L:Before:14", true, "2026-11-10T10:00:00.0000000+01:00"),
                Answer("b", "L:Before:7", true, "2026-11-10T10:00:02.0000000+01:00"),
                Answer("a", "L:After:12", true, "2026-11-10T10:00:03.0000000+01:00"),
            };

            IReadOnlyList<CheckResult> results = CheckHistory.From(lines);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(2, results[0].Answers.Count);
            Assert.AreEqual(1, results[1].Answers.Count);
        }

        [Test]
        public void From_ASecondClosingLineOfOneCheck_IsIgnored()
        {
            CheckLine[] lines =
            {
                Answer("a", "L:Before:14", true, "2026-11-10T10:00:00.0000000+01:00"),
                CheckLine.ForClosing("a", "2026-11-10T10:05:00.0000000+01:00", 1, 1),
                CheckLine.ForClosing("a", "2026-11-10T10:06:00.0000000+01:00", 0, 1),
            };

            Assert.AreEqual(1, CheckHistory.From(lines).Single().Score);
        }

        [Test]
        public void ForClosing_AScoreAboveTheItemCount_IsRefused()
        {
            Assert.Throws<System.ArgumentException>(() => CheckLine.ForClosing("a", "2026-11-10T10:05:00.0000000+01:00", 21, 20));
        }
    }
}
