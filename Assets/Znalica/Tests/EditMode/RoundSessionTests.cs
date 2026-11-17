using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class RoundSessionTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 11, 17, 10, 30, 0, TimeSpan.FromHours(1));

        private InMemoryAnswerLog _log;
        private FixedClock _clock;

        [SetUp]
        public void SetUp()
        {
            _log = new InMemoryAnswerLog();
            _clock = new FixedClock(Now);
        }

        /// <summary>Items <c>1 + 1 = ?</c>, <c>2 + 1 = ?</c>, ... : item n has the answer n + 1.</summary>
        private static List<Item> Items(int count)
        {
            var items = new List<Item>();
            for (int n = 1; n <= count; n++)
            {
                items.Add(new ArithmeticItem(n, Operation.Addition, 1, n + 1, Unknown.Result));
            }

            return items;
        }

        private RoundSession Start(IReadOnlyList<Item> items)
        {
            return new RoundSession(items, _log, _clock, 3, "round-1");
        }

        private static Answer Right(Item item)
        {
            return Answer.FromNumber(((ArithmeticItem)item).Solution);
        }

        private static Answer Wrong(Item item)
        {
            return Answer.FromNumber(((ArithmeticItem)item).Solution + 1);
        }

        private static AttemptOutcome SubmitRight(RoundSession round, int milliseconds = 1000)
        {
            return round.Submit(Right(round.CurrentItem), milliseconds);
        }

        private static AttemptOutcome SubmitWrong(RoundSession round, int milliseconds = 1000)
        {
            return round.Submit(Wrong(round.CurrentItem), milliseconds);
        }

        private string[] Asked()
        {
            return _log.ReadAll().Select(r => r.ItemKey + " " + r.Attempt + " " + (r.IsCorrect ? "right" : "wrong")).ToArray();
        }

        [Test]
        public void NewRound_AsksTheFirstItem()
        {
            List<Item> items = Items(3);

            RoundSession round = Start(items);

            Assert.AreEqual(RoundState.AwaitingAnswer, round.State);
            Assert.AreSame(items[0], round.CurrentItem);
            Assert.AreEqual(AttemptKind.First, round.NextAttempt);
            Assert.AreEqual(3, round.PlannedCount);
            Assert.AreEqual(0, round.FirstTimeRightCount);
        }

        [Test]
        public void RightAnswer_PraisesAndMakesTheNextItemCurrentAtOnce()
        {
            List<Item> items = Items(3);
            RoundSession round = Start(items);

            AttemptOutcome outcome = SubmitRight(round);

            Assert.IsTrue(outcome.IsCorrect);
            Assert.AreEqual(PhraseKey.PraiseRight, outcome.PraisePhrase);
            Assert.IsNull(outcome.Explanation);
            Assert.AreEqual(RoundState.AwaitingAnswer, round.State);
            Assert.AreSame(items[1], round.CurrentItem);
            Assert.AreEqual(AttemptKind.First, round.NextAttempt);
        }

        [Test]
        public void Submit_AppendsTheRecordBeforeItReturns_WithEveryField()
        {
            RoundSession round = Start(Items(2));

            round.Submit(Answer.FromNumber(5), 1234);

            AnswerRecord record = _log.ReadAll().Single();
            Assert.AreEqual(Activity.Arithmetic, record.Activity);
            Assert.AreEqual("A:1+1=?", record.ItemKey);
            Assert.AreEqual("5", record.Answer);
            Assert.IsFalse(record.IsCorrect);
            Assert.AreEqual(AttemptKind.First, record.Attempt);
            Assert.AreEqual(1234, record.ResponseMilliseconds);
            Assert.AreEqual(Now.ToString("o"), record.AnsweredAt);
            Assert.AreEqual(3, record.SessionNumber);
            Assert.AreEqual("round-1", record.RoundId);
        }

        [Test]
        public void WrongAnswer_ShowsAnExplanation_AndAsksTheSameItemAgain()
        {
            List<Item> items = Items(5);
            RoundSession round = Start(items);

            AttemptOutcome outcome = SubmitWrong(round);

            Assert.IsFalse(outcome.IsCorrect);
            Assert.IsNull(outcome.PraisePhrase);
            Assert.IsNotNull(outcome.Explanation);
            Assert.AreEqual(PhraseKey.ExplainJump, outcome.Explanation.Phrase);
            Assert.AreEqual((1, 1, 2), (outcome.Explanation.Start, outcome.Explanation.Change, outcome.Explanation.End));
            Assert.AreEqual(RoundState.ShowingExplanation, round.State);
            Assert.IsTrue(round.CanRetry);
            Assert.AreSame(items[0], round.CurrentItem);
            Assert.AreEqual(AttemptKind.Retry, round.NextAttempt);
        }

        [Test]
        public void WrongThenRetryRight_ReturnsTheItemThreePlacesLaterOnceMore()
        {
            List<Item> items = Items(5);
            RoundSession round = Start(items);

            SubmitWrong(round);
            SubmitRight(round);
            for (int i = 1; i <= 3; i++)
            {
                Assert.AreSame(items[i], round.CurrentItem);
                SubmitRight(round);
            }

            Assert.AreSame(items[0], round.CurrentItem, "the missed item comes back after three others");
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);
            SubmitRight(round);
            Assert.AreSame(items[4], round.CurrentItem);
            SubmitRight(round);

            Assert.AreEqual(RoundState.Ended, round.State);
            Assert.IsNull(round.CurrentItem);
            CollectionAssert.AreEqual(
                new[]
                {
                    "A:1+1=? First wrong", "A:1+1=? Retry right",
                    "A:2+1=? First right", "A:3+1=? First right", "A:4+1=? First right",
                    "A:1+1=? Return right",
                    "A:5+1=? First right",
                },
                Asked());
        }

        [Test]
        public void WrongNearTheEnd_ReturnsTheItemAtTheEnd()
        {
            List<Item> items = Items(2);
            RoundSession round = Start(items);

            SubmitWrong(round);
            SubmitRight(round);
            SubmitRight(round);

            Assert.AreSame(items[0], round.CurrentItem);
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);
            SubmitRight(round);
            Assert.AreEqual(RoundState.Ended, round.State);
        }

        [Test]
        public void WrongOnTheOnlyItem_ReturnsItRightAfterTheRetry()
        {
            RoundSession round = Start(Items(1));

            SubmitWrong(round);
            SubmitRight(round);

            Assert.AreEqual(RoundState.AwaitingAnswer, round.State);
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);
        }

        [Test]
        public void SecondWrongAnswer_ShowsTheExplanation_AndOnlyContinueGoesOn()
        {
            List<Item> items = Items(5);
            RoundSession round = Start(items);
            SubmitWrong(round);

            AttemptOutcome second = SubmitWrong(round);

            Assert.IsFalse(second.IsCorrect);
            Assert.IsNotNull(second.Explanation);
            Assert.AreEqual(RoundState.ShowingExplanation, round.State);
            Assert.IsFalse(round.CanRetry);
            Assert.AreEqual(2, _log.ReadAll().Count);
            Assert.Throws<InvalidOperationException>(() => round.Submit(Answer.FromNumber(2), 1));

            round.Continue();

            Assert.AreEqual(RoundState.AwaitingAnswer, round.State);
            Assert.AreSame(items[1], round.CurrentItem);
        }

        [Test]
        public void ItemEndedByTwoWrongAnswers_StillReturnsOnce_AndNoMoreThanOnce()
        {
            List<Item> items = Items(5);
            RoundSession round = Start(items);
            SubmitWrong(round);
            SubmitWrong(round);
            round.Continue();
            for (int i = 1; i <= 3; i++)
            {
                SubmitRight(round);
            }

            Assert.AreSame(items[0], round.CurrentItem);
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);
            SubmitWrong(round);
            Assert.AreEqual(AttemptKind.Retry, round.NextAttempt);
            SubmitWrong(round);
            round.Continue();
            SubmitRight(round);

            Assert.AreEqual(RoundState.Ended, round.State);
            string[] asked = Asked();
            Assert.AreEqual(1, asked.Count(a => a.StartsWith("A:1+1=? Return")), "one Return at most");
            Assert.AreEqual(2, asked.Count(a => a.StartsWith("A:1+1=? Retry")));
            Assert.AreEqual(1, asked.Count(a => a.StartsWith("A:1+1=? First")));
        }

        [Test]
        public void FirstTimeRightCount_CountsOnlyRightAnswersOnFirst()
        {
            RoundSession round = Start(Items(5));

            SubmitRight(round);   // item 1, first, right: counts
            SubmitWrong(round);   // item 2, first, wrong
            SubmitRight(round);   // item 2, retry, right: does not count
            SubmitRight(round);   // items 3, 4 and 5, first, right: count
            SubmitRight(round);
            SubmitRight(round);
            // The returned item 2 is now current: a right Return answer does not count either.
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);
            SubmitRight(round);

            Assert.AreEqual(RoundState.Ended, round.State);
            Assert.AreEqual(4, round.FirstTimeRightCount, "items 1, 3, 4 and 5; not the retry or the return of item 2");
        }

        [Test]
        public void FirstTimeRightCount_MatchesTheRightFirstRecordsInTheLog()
        {
            RoundSession round = Start(Items(6));

            SubmitRight(round);
            SubmitWrong(round);
            SubmitRight(round);
            SubmitWrong(round);
            SubmitWrong(round);
            round.Continue();
            while (round.State != RoundState.Ended)
            {
                SubmitRight(round);
            }

            int fromLog = _log.ReadAll().Count(r => r.IsRightFirstTime);
            Assert.AreEqual(fromLog, round.FirstTimeRightCount);
            Assert.AreEqual(4, fromLog);
            Assert.AreEqual(6, round.PlannedCount);
        }

        [Test]
        public void ARightRetry_OrReturn_IsRecordedButNeverRightFirstTime()
        {
            RoundSession round = Start(Items(1));

            SubmitWrong(round);
            SubmitRight(round);
            SubmitRight(round);

            Assert.AreEqual(0, round.FirstTimeRightCount);
            CollectionAssert.AreEqual(
                new[] { "A:1+1=? First wrong", "A:1+1=? Retry right", "A:1+1=? Return right" }, Asked());
            Assert.IsFalse(_log.ReadAll().Any(r => r.IsRightFirstTime));
        }

        [Test]
        public void WrongSubtraction_ExplainsWithAJumpBack()
        {
            var item = new ArithmeticItem(13, Operation.Subtraction, 6, 7, Unknown.Result);
            RoundSession round = Start(new Item[] { item });

            AttemptOutcome outcome = round.Submit(Answer.FromNumber(8), 100);

            Assert.AreEqual(PhraseKey.ExplainJumpBack, outcome.Explanation.Phrase);
            Assert.AreEqual((13, -6, 7), (outcome.Explanation.Start, outcome.Explanation.Change, outcome.Explanation.End));
        }

        [Test]
        public void EmptyRound_HasEnded()
        {
            RoundSession round = Start(new List<Item>());

            Assert.AreEqual(RoundState.Ended, round.State);
            Assert.IsNull(round.CurrentItem);
            Assert.Throws<InvalidOperationException>(() => round.Submit(Answer.FromNumber(1), 1));
        }

        [Test]
        public void Continue_WhileAnItemIsAsked_OrCanStillBeRetried_Throws()
        {
            RoundSession round = Start(Items(2));
            Assert.Throws<InvalidOperationException>(() => round.Continue());

            SubmitWrong(round);

            Assert.Throws<InvalidOperationException>(() => round.Continue());
        }

        [Test]
        public void Submit_NegativeResponseTime_Throws_AndRecordsNothing()
        {
            RoundSession round = Start(Items(2));

            Assert.Throws<ArgumentOutOfRangeException>(() => round.Submit(Answer.FromNumber(2), -1));

            Assert.IsEmpty(_log.ReadAll());
        }

        [Test]
        public void ARoundPlannedFromTheItemSet_PlaysToTheEndWhateverTheChildAnswers()
        {
            IReadOnlyList<Item> items = RoundPlanner.Plan(new ArithmeticItemSet(), new SeededRandom(7));
            RoundSession round = Start(items);
            bool wrongOnReturn = false;
            int retries = 0;

            // Wrong on every first ask and every Return; on a Retry, right and wrong in turn.
            int steps = 0;
            while (round.State != RoundState.Ended)
            {
                Assert.Less(steps++, 100, "the round must end");
                if (round.State == RoundState.ShowingExplanation && !round.CanRetry)
                {
                    round.Continue();
                }
                else if (round.NextAttempt == AttemptKind.Retry && retries++ % 2 == 0)
                {
                    SubmitRight(round);
                }
                else
                {
                    wrongOnReturn |= round.NextAttempt == AttemptKind.Return;
                    SubmitWrong(round);
                }
            }

            Assert.IsTrue(wrongOnReturn, "the round must have reached a wrong answer on a Return ask, or the test covers less than it says");
            IReadOnlyList<AnswerRecord> records = _log.ReadAll();
            Assert.AreEqual(
                round.PlannedCount,
                records.Where(r => r.Attempt == AttemptKind.First).Select(r => r.ItemKey).Distinct().Count(),
                "every planned item was asked once");
            foreach (IGrouping<string, AnswerRecord> perItem in records.GroupBy(r => r.ItemKey))
            {
                Assert.AreEqual(1, perItem.Count(r => r.Attempt == AttemptKind.First), perItem.Key);
                Assert.LessOrEqual(perItem.Count(r => r.Attempt == AttemptKind.Return), 1, perItem.Key);
            }
        }

        [Test]
        public void Return_ComesBackAfterExactlyThreeOtherItems_WhenThreeRemain()
        {
            List<Item> items = Items(4);
            RoundSession round = Start(items);

            SubmitWrong(round);
            SubmitRight(round);
            SubmitRight(round);
            SubmitRight(round);
            SubmitRight(round);

            Assert.AreSame(items[0], round.CurrentItem);
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);
        }

        [Test]
        public void Return_ComesBackLast_WhenOnlyTwoOtherItemsRemain()
        {
            List<Item> items = Items(3);
            RoundSession round = Start(items);

            SubmitWrong(round);
            SubmitRight(round);
            SubmitRight(round);
            SubmitRight(round);

            Assert.AreSame(items[0], round.CurrentItem);
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);
            SubmitRight(round);
            Assert.AreEqual(RoundState.Ended, round.State);
        }

        [Test]
        public void TwoReturnsQueuedAtOnce_ComeBackInTheOrderTheyWereMissed()
        {
            RoundSession round = Start(Items(6));

            SubmitWrong(round);   // item 1
            SubmitRight(round);
            SubmitWrong(round);   // item 2, while the Return of item 1 is still queued
            SubmitRight(round);
            while (round.State != RoundState.Ended)
            {
                SubmitRight(round);
            }

            CollectionAssert.AreEqual(
                new[]
                {
                    "A:1+1=? First wrong", "A:1+1=? Retry right",
                    "A:2+1=? First wrong", "A:2+1=? Retry right",
                    "A:3+1=? First right", "A:4+1=? First right",
                    "A:1+1=? Return right", "A:2+1=? Return right",
                    "A:5+1=? First right", "A:6+1=? First right",
                },
                Asked());
        }

        [Test]
        public void EachAnswer_IsStampedWithTheTimeItWasGiven()
        {
            RoundSession round = Start(Items(2));

            SubmitRight(round);
            _clock.Now = Now.AddSeconds(5);
            SubmitRight(round);

            IReadOnlyList<AnswerRecord> records = _log.ReadAll();
            Assert.AreEqual(Now.ToString("o"), records[0].AnsweredAt);
            Assert.AreEqual(Now.AddSeconds(5).ToString("o"), records[1].AnsweredAt);
        }

        [Test]
        public void Continue_AfterTheRoundEnded_Throws()
        {
            RoundSession round = Start(Items(1));
            SubmitRight(round);
            Assert.AreEqual(RoundState.Ended, round.State);

            Assert.Throws<InvalidOperationException>(() => round.Continue());
        }

        [Test]
        public void NextAttempt_IsWhatSubmitAccepts_AndNullWhenSubmitWouldThrow()
        {
            RoundSession round = Start(Items(1));
            Assert.AreEqual(AttemptKind.First, round.NextAttempt);

            SubmitWrong(round);
            Assert.AreEqual(AttemptKind.Retry, round.NextAttempt);

            SubmitWrong(round);
            Assert.IsNull(round.NextAttempt, "after a second wrong answer only Continue goes on");

            round.Continue();
            Assert.AreEqual(AttemptKind.Return, round.NextAttempt);

            SubmitRight(round);
            Assert.AreEqual(RoundState.Ended, round.State);
            Assert.IsNull(round.NextAttempt, "no stale value once the round has ended");
        }
    }
}
