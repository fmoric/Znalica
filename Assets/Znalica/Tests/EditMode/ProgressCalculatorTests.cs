using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ProgressCalculatorTests
    {
        // Wednesday 18 November 2026, noon, at +01:00; its week starts on Monday 16 November.
        private static readonly DateTimeOffset Wednesday = new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.FromHours(1));

        private static AnswerRecord Rec(
            string key = "A:8+5=?",
            bool correct = true,
            AttemptKind attempt = AttemptKind.First,
            string round = "r1",
            Activity activity = Activity.Arithmetic,
            DateTimeOffset? at = null,
            int session = 1)
        {
            return TestRecords.Make(
                sessionNumber: session,
                itemKey: key,
                isCorrect: correct,
                attempt: attempt,
                roundId: round,
                activity: activity,
                answeredAt: (at ?? Wednesday).ToString("o"));
        }

        private static ProgressReport Report(IReadOnlyList<AnswerRecord> log, Activity activity, DateTimeOffset? now = null)
        {
            IReadOnlyList<ProgressReport> all = ProgressCalculator.For(log, ItemSchedule.Build(log), now ?? Wednesday);
            return all.Single(report => report.Activity == activity);
        }

        private static string[] MissedKeys(ProgressReport report)
        {
            return report.StillMissed.Select(key => key.ToString()).ToArray();
        }

        [Test]
        public void For_GivesOneReportPerActivity_InTheOrderTheScreenShows()
        {
            IReadOnlyList<ProgressReport> all = ProgressCalculator.For(new List<AnswerRecord>(), new ItemSchedule(), Wednesday);

            CollectionAssert.AreEqual(
                new[] { Activity.Arithmetic, Activity.Comparison, Activity.NumberLine }, all.Select(report => report.Activity).ToArray());
        }

        [Test]
        public void RoundsPlayed_CountsDistinctRoundIds_OfTheActivityOnly()
        {
            var log = new List<AnswerRecord>
            {
                Rec(round: "r1"),
                Rec(round: "r1", key: "A:9+2=?"),
                Rec(round: "r1", key: "A:9+2=?", attempt: AttemptKind.Retry),
                Rec(round: "r2"),
                Rec(round: "c1", key: "C:7?12", activity: Activity.Comparison),
                Rec(round: "r1", key: "L:Before:14", activity: Activity.NumberLine),
            };

            Assert.AreEqual(2, Report(log, Activity.Arithmetic).RoundsPlayed);
            Assert.AreEqual(1, Report(log, Activity.Comparison).RoundsPlayed);
            Assert.AreEqual(1, Report(log, Activity.NumberLine).RoundsPlayed);
        }

        [Test]
        public void FirstTimeRightShare_IsRightFirstTriesOverAllFirstTries_AndRetriesAreLeftOut()
        {
            var log = new List<AnswerRecord>
            {
                Rec(key: "A:8+5=?", correct: true),
                Rec(key: "A:9+2=?", correct: true),
                Rec(key: "A:7+4=?", correct: false),
                Rec(key: "A:7+4=?", correct: true, attempt: AttemptKind.Retry),
                Rec(key: "A:6+6=?", correct: true, attempt: AttemptKind.Return),
            };

            Assert.AreEqual(2.0 / 3.0, Report(log, Activity.Arithmetic).FirstTimeRightShare.Value, 1e-9);
        }

        [Test]
        public void FirstTimeRightShare_WithNoFirstTry_IsNull_NotZero()
        {
            var log = new List<AnswerRecord> { Rec(attempt: AttemptKind.Retry) };

            Assert.IsNull(Report(log, Activity.Arithmetic).FirstTimeRightShare);
        }

        [Test]
        public void StillMissed_ListsOnlyItemsInBoxZeroOrOne()
        {
            var log = new List<AnswerRecord>
            {
                // Box 0: missed last.
                Rec(key: "A:1+1=?", correct: false, session: 1),
                // Box 1: missed, then right once.
                Rec(key: "A:2+2=?", correct: false, session: 1),
                Rec(key: "A:2+2=?", correct: true, session: 2),
                // Box 2: missed, then right twice, so no longer weak.
                Rec(key: "A:3+3=?", correct: false, session: 1),
                Rec(key: "A:3+3=?", correct: true, session: 2),
                Rec(key: "A:3+3=?", correct: true, session: 4),
            };

            Assert.AreEqual(new[] { "A:1+1=?", "A:2+2=?" }, MissedKeys(Report(log, Activity.Arithmetic)));
        }

        [Test]
        public void StillMissed_OrdersByWrongFirstTries_ThenByKey()
        {
            var log = new List<AnswerRecord>
            {
                Rec(key: "A:5+5=?", correct: false, session: 1),
                Rec(key: "A:2+2=?", correct: false, session: 1),
                Rec(key: "A:9+9=?", correct: false, session: 1),
                Rec(key: "A:9+9=?", correct: false, session: 2),
                Rec(key: "A:5+5=?", correct: false, session: 2),
                Rec(key: "A:9+9=?", correct: false, session: 3),
            };

            // 9+9 three times, 5+5 twice, 2+2 once.
            Assert.AreEqual(new[] { "A:9+9=?", "A:5+5=?", "A:2+2=?" }, MissedKeys(Report(log, Activity.Arithmetic)));
        }

        [Test]
        public void StillMissed_WithEqualCounts_IsOrderedByKeyText()
        {
            var log = new List<AnswerRecord>
            {
                Rec(key: "A:9+9=?", correct: false),
                Rec(key: "A:2+2=?", correct: false),
            };

            Assert.AreEqual(new[] { "A:2+2=?", "A:9+9=?" }, MissedKeys(Report(log, Activity.Arithmetic)));
        }

        [Test]
        public void StillMissed_HoldsAtMostFive()
        {
            var log = Enumerable.Range(1, 7).Select(n => Rec(key: "A:" + n + "+1=?", correct: false)).ToList();

            Assert.AreEqual(5, Report(log, Activity.Arithmetic).StillMissed.Count);
        }

        [Test]
        public void StillMissed_LeavesOutMissesOlderThan28Days_AndKeepsThoseOfExactly28()
        {
            var log = new List<AnswerRecord>
            {
                Rec(key: "A:1+1=?", correct: false, at: Wednesday.AddDays(-28).AddSeconds(-1)),
                Rec(key: "A:2+2=?", correct: false, at: Wednesday.AddDays(-28)),
                Rec(key: "A:3+3=?", correct: false, at: Wednesday.AddDays(-27)),
            };

            Assert.AreEqual(new[] { "A:2+2=?", "A:3+3=?" }, MissedKeys(Report(log, Activity.Arithmetic)));
        }

        [Test]
        public void StillMissed_CountsWrongFirstTriesOnly_NotWrongRetries()
        {
            var log = new List<AnswerRecord>
            {
                Rec(key: "A:1+1=?", correct: false, attempt: AttemptKind.First),
                Rec(key: "A:1+1=?", correct: false, attempt: AttemptKind.Retry),
                Rec(key: "A:2+2=?", correct: false, attempt: AttemptKind.First, session: 2),
                Rec(key: "A:2+2=?", correct: false, attempt: AttemptKind.First, session: 3),
            };

            // 1+1 has one wrong First, 2+2 has two: the wrong Retry does not lift 1+1 to a tie.
            Assert.AreEqual(new[] { "A:2+2=?", "A:1+1=?" }, MissedKeys(Report(log, Activity.Arithmetic)));
        }

        [Test]
        public void StillMissed_IsPerActivity()
        {
            var log = new List<AnswerRecord>
            {
                Rec(key: "A:1+1=?", correct: false),
                Rec(key: "C:7?12", correct: false, activity: Activity.Comparison),
            };

            Assert.AreEqual(new[] { "A:1+1=?" }, MissedKeys(Report(log, Activity.Arithmetic)));
            Assert.AreEqual(new[] { "C:7?12" }, MissedKeys(Report(log, Activity.Comparison)));
            Assert.IsEmpty(Report(log, Activity.NumberLine).StillMissed);
        }

        [Test]
        public void WeeklyPractice_GivesSixWeeks_OldestFirst_EachStartingOnAMonday()
        {
            IReadOnlyList<WeekPractice> weeks = Report(new List<AnswerRecord>(), Activity.Arithmetic).WeeklyPractice;

            Assert.AreEqual(6, weeks.Count);
            Assert.AreEqual(new DateTime(2026, 10, 12), weeks[0].Monday);
            Assert.AreEqual(new DateTime(2026, 11, 16), weeks[5].Monday);
            Assert.IsTrue(weeks.All(week => week.Monday.DayOfWeek == DayOfWeek.Monday));
        }

        [Test]
        public void WeeklyPractice_SplitsAtTheMondayBoundary()
        {
            var log = new List<AnswerRecord>
            {
                // Sunday 15 November 23:59:59 belongs to the week before; Monday 16 November 00:00 starts the current one.
                Rec(round: "sunday", at: new DateTimeOffset(2026, 11, 15, 23, 59, 59, TimeSpan.FromHours(1))),
                Rec(round: "monday", at: new DateTimeOffset(2026, 11, 16, 0, 0, 0, TimeSpan.FromHours(1))),
                Rec(round: "monday", at: new DateTimeOffset(2026, 11, 17, 9, 0, 0, TimeSpan.FromHours(1))),
            };

            IReadOnlyList<WeekPractice> weeks = Report(log, Activity.Arithmetic).WeeklyPractice;

            Assert.AreEqual((1, 1), (weeks[4].Rounds, weeks[4].Answers));
            Assert.AreEqual((1, 2), (weeks[5].Rounds, weeks[5].Answers));
        }

        [Test]
        public void WeeklyPractice_WhenNowIsAMonday_TheCurrentWeekIsJustStarted()
        {
            var monday = new DateTimeOffset(2026, 11, 16, 0, 30, 0, TimeSpan.FromHours(1));
            var log = new List<AnswerRecord>
            {
                Rec(at: monday.AddMinutes(-31)),
                Rec(at: monday.AddMinutes(-1)),
                Rec(at: monday),
            };

            IReadOnlyList<WeekPractice> weeks = Report(log, Activity.Arithmetic, monday).WeeklyPractice;

            Assert.AreEqual(new DateTime(2026, 11, 16), weeks[5].Monday);
            Assert.AreEqual(2, weeks[5].Answers);
            Assert.AreEqual(1, weeks[4].Answers);
        }

        [Test]
        public void WeeklyPractice_WhenNowIsASunday_TheCurrentWeekReachesBackToItsMonday()
        {
            var sunday = new DateTimeOffset(2026, 11, 22, 23, 30, 0, TimeSpan.FromHours(1));
            var log = new List<AnswerRecord>
            {
                Rec(at: new DateTimeOffset(2026, 11, 16, 8, 0, 0, TimeSpan.FromHours(1))),
                Rec(at: new DateTimeOffset(2026, 11, 15, 8, 0, 0, TimeSpan.FromHours(1))),
            };

            IReadOnlyList<WeekPractice> weeks = Report(log, Activity.Arithmetic, sunday).WeeklyPractice;

            Assert.AreEqual(new DateTime(2026, 11, 16), weeks[5].Monday);
            Assert.AreEqual(1, weeks[5].Answers);
            Assert.AreEqual(1, weeks[4].Answers);
        }

        [Test]
        public void WeeklyPractice_LeavesOutAnswersBeforeTheSixWeeks_AndKeepsTheOldestWeek()
        {
            var log = new List<AnswerRecord>
            {
                // The oldest week shown starts on Monday 12 October.
                Rec(at: new DateTimeOffset(2026, 10, 11, 23, 0, 0, TimeSpan.FromHours(1))),
                Rec(at: new DateTimeOffset(2026, 10, 12, 0, 0, 0, TimeSpan.FromHours(1))),
            };

            IReadOnlyList<WeekPractice> weeks = Report(log, Activity.Arithmetic).WeeklyPractice;

            Assert.AreEqual(new[] { 1, 0, 0, 0, 0, 0 }, weeks.Select(week => week.Answers).ToArray());
        }

        [Test]
        public void WeeklyPractice_CountsRoundsOnceAWeek_AndEveryAnswer()
        {
            var log = new List<AnswerRecord>
            {
                Rec(round: "r1", key: "A:1+1=?"),
                Rec(round: "r1", key: "A:2+2=?"),
                Rec(round: "r1", key: "A:2+2=?", attempt: AttemptKind.Retry),
                Rec(round: "r2", key: "A:3+3=?"),
            };

            WeekPractice week = Report(log, Activity.Arithmetic).WeeklyPractice[5];

            Assert.AreEqual(2, week.Rounds);
            Assert.AreEqual(4, week.Answers);
        }

        [Test]
        public void WeeklyPractice_ReadsADateInAnotherOffsetOnTheClockOfNow()
        {
            // 23:30 on Sunday 15 November at -05:00 is 05:30 on Monday 16 November at +01:00.
            var log = new List<AnswerRecord> { Rec(at: new DateTimeOffset(2026, 11, 15, 23, 30, 0, TimeSpan.FromHours(-5))) };

            IReadOnlyList<WeekPractice> weeks = Report(log, Activity.Arithmetic).WeeklyPractice;

            Assert.AreEqual(1, weeks[5].Answers);
            Assert.AreEqual(0, weeks[4].Answers);
        }

        [Test]
        public void ADateThatCannotBeRead_StillCountsInRoundsAndShare_ButNotInAWeekOrTheMissedList()
        {
            AnswerRecord broken = TestRecords.Make(itemKey: "A:1+1=?", isCorrect: false, answeredAt: "not a date");

            ProgressReport report = Report(new List<AnswerRecord> { broken }, Activity.Arithmetic);

            Assert.AreEqual(1, report.RoundsPlayed);
            Assert.AreEqual(0.0, report.FirstTimeRightShare.Value);
            Assert.IsEmpty(report.StillMissed);
            Assert.IsTrue(report.WeeklyPractice.All(week => week.Answers == 0));
        }

        [Test]
        public void AnActivityWithNoAnswers_IsEmpty()
        {
            var log = new List<AnswerRecord> { Rec() };

            ProgressReport report = Report(log, Activity.NumberLine);

            Assert.AreEqual(0, report.RoundsPlayed);
            Assert.IsNull(report.FirstTimeRightShare);
            Assert.IsEmpty(report.StillMissed);
            Assert.AreEqual(6, report.WeeklyPractice.Count);
            Assert.IsTrue(report.WeeklyPractice.All(week => week.Rounds == 0 && week.Answers == 0));
        }

        [Test]
        public void ForAnEmptyLog_EveryReportIsEmpty()
        {
            IReadOnlyList<ProgressReport> all = ProgressCalculator.For(new List<AnswerRecord>(), new ItemSchedule(), Wednesday);

            Assert.IsTrue(all.All(report => report.RoundsPlayed == 0 && report.FirstTimeRightShare == null && report.StillMissed.Count == 0));
        }
    }
}
