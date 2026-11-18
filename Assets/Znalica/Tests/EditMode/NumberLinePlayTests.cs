using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class NumberLinePlayTests
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

        private NumberLinePlay Play(params Item[] items)
        {
            return new NumberLinePlay(new RoundSession(items, _log, _schedule, _clock, 1, "round-1"), _clock);
        }

        private string[] Asked()
        {
            return _log.ReadAll().Select(r => r.ItemKey + " " + r.Attempt + " " + r.Answer + " " + (r.IsCorrect ? "right" : "wrong")).ToArray();
        }

        /// <summary>Types the digits and presses U redu; returns the feedback of that press.</summary>
        private static Feedback Type(NumberLinePlay play, string digits)
        {
            foreach (char digit in digits)
            {
                play.Press(AnswerKey.Digit0 + (digit - '0'));
            }

            return play.Press(AnswerKey.Confirm);
        }

        [TestCase(14.0, 14)]
        [TestCase(14.49, 14)]
        [TestCase(13.51, 14)]
        [TestCase(13.0, 13)]
        [TestCase(0.0, 0)]
        [TestCase(20.0, 20)]
        [TestCase(0.4, 0)]
        [TestCase(0.6, 1)]
        [TestCase(19.6, 20)]
        public void NearestNumber_IsTheWholeNumberClosestToTheClick(double position, int expected)
        {
            Assert.AreEqual(expected, NumberLinePlay.NearestNumber(position));
        }

        [TestCase(-0.4, 0)]
        [TestCase(-3.0, 0)]
        [TestCase(20.4, 20)]
        [TestCase(40.0, 20)]
        [TestCase(double.PositiveInfinity, 20)]
        [TestCase(double.NegativeInfinity, 0)]
        public void NearestNumber_KeepsAClickPastAnEndOnThatEnd(double position, int expected)
        {
            Assert.AreEqual(expected, NumberLinePlay.NearestNumber(position));
        }

        [Test]
        public void PlaceAt_ANumberNearestTheTarget_IsRight_AndMovesOnAtOnce()
        {
            NumberLinePlay play = Play(NumberLineItem.Place(14), NumberLineItem.Place(3));

            Feedback feedback = play.PlaceAt(14.3);

            Assert.AreEqual(FeedbackKind.Praise, feedback.Kind);
            Assert.AreEqual("L:Place:3", play.CurrentItem.Key.ToString());
            CollectionAssert.AreEqual(new[] { "L:Place:14 First 14 right" }, Asked());
        }

        [Test]
        public void PlaceAt_AClickCloserToTheNextNumber_IsWrong_AndShowsThePositionExplanation()
        {
            NumberLinePlay play = Play(NumberLineItem.Place(14), NumberLineItem.Place(3));

            Feedback feedback = play.PlaceAt(14.6);

            Assert.AreEqual(FeedbackKind.Explanation, feedback.Kind);
            Assert.AreEqual(ExplanationKind.PositionOnLine, feedback.Explanation.Kind);
            Assert.AreEqual(14, feedback.Explanation.Target);
            Assert.AreEqual(15, play.LastPlaced, "The picture can show where the child clicked.");
            Assert.AreEqual("L:Place:14", play.CurrentItem.Key.ToString(), "The same item is asked again.");
            Assert.IsFalse(play.AwaitsContinue);

            play.PlaceAt(13.8);

            CollectionAssert.AreEqual(new[] { "L:Place:14 First 15 wrong", "L:Place:14 Retry 14 right" }, Asked());
        }

        [Test]
        public void ASecondWrongClick_LeavesOnlyContinue_AndClicksAndKeysDoNothingUntilThen()
        {
            NumberLinePlay play = Play(NumberLineItem.Place(14), NumberLineItem.Place(3));
            play.PlaceAt(2.0);
            play.PlaceAt(5.0);

            Assert.IsTrue(play.AwaitsContinue);
            Assert.IsNull(play.PlaceAt(14.0));
            Assert.IsNull(play.Press(AnswerKey.Confirm));
            Assert.AreEqual(2, Asked().Length);

            play.Press(AnswerKey.Continue);

            Assert.AreEqual("L:Place:3", play.CurrentItem.Key.ToString());
            Assert.IsNull(play.Feedback);
        }

        [Test]
        public void ADigitAndConfirm_DoNothingForAPlaceItem_SoATypedNumberIsNeverItsAnswer()
        {
            NumberLinePlay play = Play(NumberLineItem.Place(14), NumberLineItem.Place(3));

            Assert.IsNull(play.Press(AnswerKey.Digit1));
            Assert.IsNull(play.Press(AnswerKey.Digit4));
            Assert.IsNull(play.Press(AnswerKey.Confirm));

            Assert.IsNull(play.Entered);
            Assert.IsEmpty(Asked());
            Assert.AreEqual("L:Place:14", play.CurrentItem.Key.ToString());
        }

        [Test]
        public void AClickOnTheLine_DoesNothingForATypedItem()
        {
            NumberLinePlay play = Play(NumberLineItem.Before(14), NumberLineItem.Place(3));

            Assert.IsNull(play.PlaceAt(13.0));
            Assert.IsNull(play.PlaceAt(double.NaN));

            Assert.IsEmpty(Asked());
            Assert.AreEqual("L:Before:14", play.CurrentItem.Key.ToString());
        }

        [Test]
        public void Before_IsTypedOnThePad_AndTheRightNumberIsTheOneBefore()
        {
            NumberLinePlay play = Play(NumberLineItem.Before(14), NumberLineItem.After(14));

            play.Press(AnswerKey.Digit1);
            play.Press(AnswerKey.Digit3);
            Assert.AreEqual(13, play.Entered);
            Feedback right = play.Press(AnswerKey.Confirm);

            Assert.AreEqual(FeedbackKind.Praise, right.Kind);
            Type(play, "13");
            Type(play, "15");
            CollectionAssert.AreEqual(
                new[] { "L:Before:14 First 13 right", "L:After:14 First 13 wrong", "L:After:14 Retry 15 right" }, Asked());
        }

        [Test]
        public void AWrongJump_ShowsTheJumpOnTheLine_AndALandingAboveTwentyCannotBeTyped()
        {
            NumberLinePlay play = Play(NumberLineItem.Jump(6, 3), NumberLineItem.Place(3));

            play.Press(AnswerKey.Digit2);
            play.Press(AnswerKey.Digit0);
            play.Press(AnswerKey.Digit5);
            Assert.AreEqual(20, play.Entered, "The box refuses a digit that would pass 20.");
            play.Press(AnswerKey.Erase);
            play.Press(AnswerKey.Erase);
            Feedback wrong = Type(play, "8");

            Assert.AreEqual(ExplanationKind.NumberLineJump, wrong.Explanation.Kind);
            Assert.AreEqual((6, 3, 9), (wrong.Explanation.Start, wrong.Explanation.Change, wrong.Explanation.End));
            Assert.AreEqual(PhraseKey.ExplainJump, wrong.Phrase);
        }

        [Test]
        public void ABackwardsJump_IsExplainedAsAJumpBack()
        {
            Explanation explanation = NumberLineItem.Jump(9, -3).Explain();

            Assert.AreEqual(PhraseKey.ExplainJumpBack, explanation.Phrase);
            Assert.AreEqual((9, -3, 6, true), (explanation.Start, explanation.Change, explanation.End, explanation.Backwards));
        }

        [Test]
        public void ARoundPlayedToTheEnd_ReportsTheFirstTimeRightCount_AndEnds()
        {
            NumberLinePlay play = Play(NumberLineItem.Place(1), NumberLineItem.Before(5), NumberLineItem.Jump(6, 3));

            play.PlaceAt(1.0);
            Type(play, "4");
            Type(play, "1");
            Type(play, "9");
            Assert.IsFalse(play.IsEnded, "A missed item comes back once more in the round.");
            Type(play, "9");

            Assert.IsTrue(play.IsEnded);
            Assert.AreEqual("2", play.SummaryArgs["Right"]);
            Assert.AreEqual("3", play.SummaryArgs["Total"]);
            Assert.IsNull(play.PlaceAt(1.0), "Nothing is asked once the round has ended.");
            Assert.IsNull(play.Press(AnswerKey.Confirm));
        }

        [Test]
        public void ASaveThatFails_LeavesTheClickUnanswered_SoTheChildCanClickAgain()
        {
            var failing = new FailingAnswerLog { FailNext = true };
            var play = new NumberLinePlay(
                new RoundSession(new Item[] { NumberLineItem.Place(14), NumberLineItem.Place(3) }, failing, _schedule, _clock, 1, "round-1"), _clock);

            Assert.Throws<System.IO.IOException>(() => play.PlaceAt(14.0));

            Assert.AreEqual("L:Place:14", play.CurrentItem.Key.ToString());
            Assert.IsNull(play.LastPlaced, "A click that was not kept is not the child's mark.");
            Assert.IsNull(play.Feedback);
            Assert.IsNull(_schedule.Get(NumberLineItem.Place(14).Key));
        }
    }
}
