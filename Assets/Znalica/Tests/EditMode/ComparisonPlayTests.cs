using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ComparisonPlayTests
    {
        private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 11, 18, 10, 30, 0, TimeSpan.FromHours(1));

        private InMemoryAnswerLog _log;
        private FixedClock _clock;
        private ItemSchedule _schedule;

        [SetUp]
        public void SetUp()
        {
            _log = new InMemoryAnswerLog();
            _clock = new FixedClock(Start);
            _schedule = new ItemSchedule();
        }

        private ComparisonPlay Play(IReadOnlyList<Item> items, IAnswerLog log = null)
        {
            return new ComparisonPlay(new RoundSession(items, log ?? _log, _schedule, _clock, 1, "round-1"), _clock);
        }

        private static Item Less(int left, int right)
        {
            return ComparisonItem.ChooseSign(Representation.Numbers, left, right);
        }

        private static Item Order(params int[] numbers)
        {
            return ComparisonItem.PutInOrder(numbers);
        }

        private string[] Asked()
        {
            return _log.ReadAll().Select(r => r.ItemKey + " " + r.Attempt + " " + r.Answer + " " + (r.IsCorrect ? "right" : "wrong")).ToArray();
        }

        [Test]
        public void ChooseSign_WithTheRightSign_GivesPraiseAndMovesOnAtOnce()
        {
            ComparisonPlay play = Play(new[] { Less(3, 7), Less(9, 2) });

            Feedback feedback = play.ChooseSign(Sign.Less);

            Assert.AreEqual(FeedbackKind.Praise, feedback.Kind);
            Assert.AreEqual("C:9?2", play.CurrentItem.Key.ToString());
            Assert.AreSame(feedback, play.Feedback);
            CollectionAssert.AreEqual(new[] { "C:3?7 First Less right" }, Asked());
        }

        [Test]
        public void ChooseSign_WithAWrongSign_ShowsTheSideBySideExplanation_AndAsksTheSameItemAgain()
        {
            ComparisonPlay play = Play(new[] { Less(3, 7), Less(9, 2) });

            Feedback feedback = play.ChooseSign(Sign.Greater);

            Assert.AreEqual(FeedbackKind.Explanation, feedback.Kind);
            Assert.AreEqual(ExplanationKind.SideBySide, feedback.Explanation.Kind);
            Assert.AreEqual("C:3?7", play.CurrentItem.Key.ToString());
            Assert.IsFalse(play.AwaitsContinue);

            play.ChooseSign(Sign.Less);

            CollectionAssert.AreEqual(new[] { "C:3?7 First Greater wrong", "C:3?7 Retry Less right" }, Asked());
        }

        [Test]
        public void ASecondWrongSign_LeavesOnlyContinue_AndClicksDoNothingUntilThen()
        {
            ComparisonPlay play = Play(new[] { Less(3, 7), Less(9, 2) });
            play.ChooseSign(Sign.Greater);
            play.ChooseSign(Sign.Equal);

            Assert.IsTrue(play.AwaitsContinue);
            Assert.IsNull(play.ChooseSign(Sign.Less), "A sign click must not skip the explanation unseen.");
            Assert.AreEqual(2, _log.ReadAll().Count);

            play.Continue();

            Assert.IsFalse(play.AwaitsContinue);
            Assert.IsNull(play.Feedback);
            Assert.AreEqual("C:9?2", play.CurrentItem.Key.ToString());
        }

        [Test]
        public void ResponseTime_RunsFromTheItemBeingShownToTheClick()
        {
            ComparisonPlay play = Play(new[] { Less(3, 7), Less(9, 2) });
            _clock.Now = Start.AddMilliseconds(2500);
            play.ChooseSign(Sign.Less);
            _clock.Now = Start.AddMilliseconds(2500 + 700);

            play.ChooseSign(Sign.Greater);

            CollectionAssert.AreEqual(new[] { 2500, 700 }, _log.ReadAll().Select(r => r.ResponseMilliseconds).ToArray());
        }

        [Test]
        public void SignClickOnAnOrderingItem_AndNumberClickOnASignItem_DoNothing()
        {
            ComparisonPlay ordering = Play(new[] { Order(3, 9, 14) });
            Assert.IsNull(ordering.ChooseSign(Sign.Less));
            Assert.AreEqual(0, _log.ReadAll().Count);

            ComparisonPlay sign = Play(new[] { Less(3, 7) });
            Assert.IsNull(sign.PickNumber(3));
            sign.UndoPick();
            Assert.AreEqual(0, _log.ReadAll().Count);
            Assert.AreEqual("C:3?7", sign.CurrentItem.Key.ToString());
        }

        [Test]
        public void PickNumber_TheLastNumberOfTheRightOrder_GivesTheAnswerAndPraise()
        {
            ComparisonPlay play = Play(new[] { Order(14, 3, 9), Less(1, 2) });

            Assert.IsNull(play.PickNumber(3));
            Assert.IsNull(play.PickNumber(9));
            CollectionAssert.AreEqual(new[] { 3, 9 }, play.Picked);
            Assert.AreEqual(0, _log.ReadAll().Count, "Nothing is answered before the last number is clicked.");

            Feedback feedback = play.PickNumber(14);

            Assert.AreEqual(FeedbackKind.Praise, feedback.Kind);
            CollectionAssert.AreEqual(new[] { "O:3,9,14 First 3,9,14 right" }, Asked());
            Assert.AreEqual(0, play.Picked.Count);
            Assert.AreEqual("C:1?2", play.CurrentItem.Key.ToString());
        }

        [Test]
        public void PickNumber_InAWrongOrder_ShowsTheOrderedSequence_ThenAsksAgainFromTheStart()
        {
            ComparisonPlay play = Play(new[] { Order(14, 3, 9) });
            play.PickNumber(14);
            play.PickNumber(3);

            Feedback feedback = play.PickNumber(9);

            Assert.AreEqual(ExplanationKind.OrderedSequence, feedback.Explanation.Kind);
            CollectionAssert.AreEqual(new[] { 3, 9, 14 }, feedback.Explanation.Numbers);
            Assert.AreEqual(0, play.Picked.Count);
            Assert.AreEqual("O:3,9,14", play.CurrentItem.Key.ToString());
            CollectionAssert.AreEqual(new[] { "O:3,9,14 First 14,3,9 wrong" }, Asked());

            play.PickNumber(3);
            play.PickNumber(9);
            play.PickNumber(14);

            Assert.AreEqual("O:3,9,14 Retry 3,9,14 right", Asked().Last());
        }

        [Test]
        public void PickNumber_IgnoresANumberTwiceAndANumberNotInTheItem()
        {
            ComparisonPlay play = Play(new[] { Order(3, 9, 14) });
            play.PickNumber(9);

            Assert.IsNull(play.PickNumber(9));
            Assert.IsNull(play.PickNumber(5));

            CollectionAssert.AreEqual(new[] { 9 }, play.Picked);
        }

        [Test]
        public void UndoPick_TakesBackTheLastClick_AndDoesNothingWhenThereIsNone()
        {
            ComparisonPlay play = Play(new[] { Order(3, 9, 14) });
            play.UndoPick();
            play.PickNumber(9);
            play.PickNumber(3);

            play.UndoPick();

            CollectionAssert.AreEqual(new[] { 9 }, play.Picked);

            play.UndoPick();
            play.UndoPick();

            Assert.AreEqual(0, play.Picked.Count);
        }

        [Test]
        public void FirstClickOfTheNextAnswer_ClearsPraise_ButAnExplanationStays()
        {
            ComparisonPlay play = Play(new[] { Less(3, 7), Order(3, 9, 14) });
            play.ChooseSign(Sign.Less);
            Assert.AreEqual(FeedbackKind.Praise, play.Feedback.Kind);

            play.PickNumber(3);

            Assert.IsNull(play.Feedback);

            ComparisonPlay wrong = Play(new[] { Order(3, 9, 14) });
            wrong.PickNumber(14);
            wrong.PickNumber(9);
            wrong.PickNumber(3);
            Assert.AreEqual(FeedbackKind.Explanation, wrong.Feedback.Kind);

            wrong.PickNumber(3);

            Assert.AreEqual(FeedbackKind.Explanation, wrong.Feedback.Kind, "The explanation stays until the retry is answered.");
        }

        [Test]
        public void ASignClickWhoseAnswerCannotBeSaved_ChangesNothing_AndCanBeClickedAgain()
        {
            var failing = new FailingAnswerLog();
            ComparisonPlay play = Play(new[] { Less(3, 7), Less(9, 2) }, failing);
            failing.FailNext = true;

            Assert.Throws<System.IO.IOException>(() => play.ChooseSign(Sign.Less));

            Assert.AreEqual("C:3?7", play.CurrentItem.Key.ToString());
            Assert.IsNull(play.Feedback);
            Assert.IsNull(_schedule.Get(play.CurrentItem.Key));

            play.ChooseSign(Sign.Less);

            Assert.AreEqual("C:9?2", play.CurrentItem.Key.ToString());
        }

        [Test]
        public void TheLastNumberClickWhoseAnswerCannotBeSaved_LeavesTheEarlierClicksAsTheyWere()
        {
            var failing = new FailingAnswerLog();
            ComparisonPlay play = Play(new[] { Order(3, 9, 14) }, failing);
            play.PickNumber(3);
            play.PickNumber(9);
            failing.FailNext = true;

            Assert.Throws<System.IO.IOException>(() => play.PickNumber(14));

            CollectionAssert.AreEqual(new[] { 3, 9 }, play.Picked);

            play.PickNumber(14);

            Assert.IsTrue(play.IsEnded);
        }

        [Test]
        public void ARoundOfBothTasks_PlaysToTheEnd_AndCountsOnlyTheRightFirstAnswers()
        {
            ComparisonPlay play = Play(new[] { Less(3, 7), Order(3, 9, 14), Less(9, 2) });
            play.ChooseSign(Sign.Less);
            play.PickNumber(14);
            play.PickNumber(9);
            play.PickNumber(3);
            play.PickNumber(3);
            play.PickNumber(9);
            play.PickNumber(14);
            play.ChooseSign(Sign.Greater);
            // The wrong ordering item comes back after three other items, or last: here it is the last one.
            Assert.AreEqual("O:3,9,14", play.CurrentItem.Key.ToString());
            play.PickNumber(3);
            play.PickNumber(9);
            play.PickNumber(14);

            Assert.IsTrue(play.IsEnded);
            Assert.AreEqual("2", play.SummaryArgs["Right"]);
            Assert.AreEqual("3", play.SummaryArgs["Total"]);
        }
    }
}
