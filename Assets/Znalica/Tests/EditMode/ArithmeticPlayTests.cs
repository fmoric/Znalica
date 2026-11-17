using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ArithmeticPlayTests
    {
        private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 11, 17, 10, 30, 0, TimeSpan.FromHours(1));

        private InMemoryAnswerLog _log;
        private FixedClock _clock;

        [SetUp]
        public void SetUp()
        {
            _log = new InMemoryAnswerLog();
            _clock = new FixedClock(Start);
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

        private ArithmeticPlay Play(IReadOnlyList<Item> items)
        {
            return new ArithmeticPlay(new RoundSession(items, _log, _clock, 1, "round-1"), _clock);
        }

        private static AnswerKey Digit(int digit)
        {
            return AnswerKey.Digit0 + digit;
        }

        /// <summary>Types the number digit by digit and confirms; returns what Confirm produced.</summary>
        private static Feedback Enter(ArithmeticPlay play, int number)
        {
            foreach (char digit in number.ToString())
            {
                play.Press(Digit(digit - '0'));
            }

            return play.Press(AnswerKey.Confirm);
        }

        private static int Solution(Item item)
        {
            return ((ArithmeticItem)item).Solution;
        }

        private static Feedback EnterRight(ArithmeticPlay play)
        {
            return Enter(play, Solution(play.CurrentItem));
        }

        private static Feedback EnterWrong(ArithmeticPlay play)
        {
            return Enter(play, Solution(play.CurrentItem) + 1);
        }

        private string[] Asked()
        {
            return _log.ReadAll().Select(r => r.ItemKey + " " + r.Attempt + " " + (r.IsCorrect ? "right" : "wrong")).ToArray();
        }

        [Test]
        public void EachDigitKey_PutsItsOwnDigitInTheBox()
        {
            for (int digit = 0; digit <= 9; digit++)
            {
                ArithmeticPlay play = Play(Items(2));

                play.Press(Digit(digit));

                Assert.AreEqual(digit, play.Entered, "Digit key " + digit);
            }
        }

        [Test]
        public void DigitThatWouldPassTwenty_IsRefused()
        {
            ArithmeticPlay play = Play(Items(2));

            play.Press(AnswerKey.Digit2);
            play.Press(AnswerKey.Digit5);
            Assert.AreEqual(2, play.Entered);

            play.Press(AnswerKey.Digit0);
            Assert.AreEqual(20, play.Entered);
        }

        [Test]
        public void Erase_RemovesTheLastDigit()
        {
            ArithmeticPlay play = Play(Items(2));
            play.Press(AnswerKey.Digit1);
            play.Press(AnswerKey.Digit5);

            play.Press(AnswerKey.Erase);
            Assert.AreEqual(1, play.Entered);

            play.Press(AnswerKey.Erase);
            Assert.IsNull(play.Entered);
        }

        [Test]
        public void Confirm_WithAnEmptyBox_SubmitsNothing()
        {
            ArithmeticPlay play = Play(Items(2));

            Feedback feedback = play.Press(AnswerKey.Confirm);

            Assert.IsNull(feedback);
            Assert.IsNull(play.Feedback);
            Assert.IsEmpty(_log.ReadAll());
        }

        [Test]
        public void RightAnswer_GivesPraise_EmptiesTheBoxAndMakesTheNextItemCurrent()
        {
            List<Item> items = Items(3);
            ArithmeticPlay play = Play(items);

            Feedback feedback = EnterRight(play);

            Assert.AreEqual(FeedbackKind.Praise, feedback.Kind);
            Assert.AreEqual(PhraseKey.PraiseRight, feedback.Phrase);
            Assert.IsNull(feedback.Explanation);
            Assert.AreSame(feedback, play.Feedback);
            Assert.IsNull(play.Entered);
            Assert.AreSame(items[1], play.CurrentItem);
            Assert.IsFalse(play.AwaitsContinue);
        }

        [Test]
        public void Praise_DoesNotHoldTheNextItem()
        {
            ArithmeticPlay play = Play(Items(3));
            EnterRight(play);

            // No Continue and no wait: the very next press already answers the next item.
            Feedback next = EnterRight(play);

            Assert.AreEqual(FeedbackKind.Praise, next.Kind);
            Assert.AreEqual(new[] { "A:1+1=? First right", "A:2+1=? First right" }, Asked());
        }

        [Test]
        public void WrongAnswer_ShowsTheJump_EmptiesTheBoxAndAsksTheSameItemAgain()
        {
            List<Item> items = Items(3);
            ArithmeticPlay play = Play(items);
            play.Press(AnswerKey.Digit9);
            play.Press(AnswerKey.Confirm);

            Feedback feedback = play.Feedback;

            Assert.AreEqual(FeedbackKind.Explanation, feedback.Kind);
            Assert.AreEqual(PhraseKey.ExplainJump, feedback.Phrase);
            Assert.AreEqual(new[] { 1, 1, 2 }, new[] { feedback.Explanation.Start, feedback.Explanation.Change, feedback.Explanation.End });
            Assert.AreEqual("1", feedback.Args["Start"]);
            Assert.AreEqual("1", feedback.Args["Change"]);
            Assert.AreEqual("2", feedback.Args["End"]);
            Assert.IsNull(play.Entered);
            Assert.AreSame(items[0], play.CurrentItem);
            Assert.IsFalse(play.AwaitsContinue);
        }

        [Test]
        public void WrongSubtraction_UsesTheJumpBackPhraseWithTheSizeOfTheJump()
        {
            var items = new List<Item> { new ArithmeticItem(13, Operation.Subtraction, 6, 7, Unknown.Result) };
            ArithmeticPlay play = Play(items);

            Feedback feedback = Enter(play, 8);

            Assert.AreEqual(PhraseKey.ExplainJumpBack, feedback.Phrase);
            Assert.AreEqual("13", feedback.Args["Start"]);
            Assert.AreEqual("6", feedback.Args["Change"]);
            Assert.AreEqual("7", feedback.Args["End"]);
            Assert.AreEqual(-6, feedback.Explanation.Change);
        }

        [Test]
        public void RightAnswerAfterTheExplanation_IsTheRetry_ReplacesTheExplanationWithPraiseAndMovesOn()
        {
            List<Item> items = Items(3);
            ArithmeticPlay play = Play(items);
            EnterWrong(play);

            Feedback feedback = EnterRight(play);

            Assert.AreEqual(FeedbackKind.Praise, feedback.Kind);
            Assert.AreSame(feedback, play.Feedback);
            Assert.AreSame(items[1], play.CurrentItem);
            Assert.AreEqual(new[] { "A:1+1=? First wrong", "A:1+1=? Retry right" }, Asked());
        }

        [Test]
        public void SecondWrongAnswer_ShowsTheExplanationAgain_AndOnlyConfirmGoesOn()
        {
            List<Item> items = Items(3);
            ArithmeticPlay play = Play(items);
            EnterWrong(play);

            Feedback second = EnterWrong(play);

            Assert.AreEqual(FeedbackKind.Explanation, second.Kind);
            Assert.IsTrue(play.AwaitsContinue);

            play.Press(AnswerKey.Digit5);
            play.Press(AnswerKey.Erase);
            Assert.IsNull(play.Entered);
            Assert.AreSame(items[0], play.CurrentItem);
            Assert.AreEqual(2, _log.ReadAll().Count);

            Feedback onward = play.Press(AnswerKey.Confirm);

            Assert.IsNull(onward);
            Assert.IsNull(play.Feedback);
            Assert.IsFalse(play.AwaitsContinue);
            Assert.AreSame(items[1], play.CurrentItem);
            Assert.AreEqual(2, _log.ReadAll().Count, "Going on is not an answer.");
        }

        [Test]
        public void MissedItem_ReturnsAfterThreeOthers_AWrongReturnGetsOneRetryAndNoSecondReturn()
        {
            List<Item> items = Items(6);
            ArithmeticPlay play = Play(items);

            EnterWrong(play);
            EnterRight(play);
            for (int other = 0; other < 3; other++)
            {
                EnterRight(play);
            }

            Assert.AreSame(items[0], play.CurrentItem, "The missed item comes back after three others.");

            Feedback wrongReturn = EnterWrong(play);
            Assert.AreEqual(FeedbackKind.Explanation, wrongReturn.Kind);
            Assert.IsFalse(play.AwaitsContinue, "A wrong Return still gets a Retry.");

            EnterWrong(play);
            Assert.IsTrue(play.AwaitsContinue);
            play.Press(AnswerKey.Confirm);

            Assert.AreSame(items[4], play.CurrentItem);
            EnterRight(play);
            EnterRight(play);
            Assert.IsTrue(play.IsEnded, "No second Return.");
            Assert.AreEqual(
                new[]
                {
                    "A:1+1=? First wrong",
                    "A:1+1=? Retry right",
                    "A:2+1=? First right",
                    "A:3+1=? First right",
                    "A:4+1=? First right",
                    "A:1+1=? Return wrong",
                    "A:1+1=? Retry wrong",
                    "A:5+1=? First right",
                    "A:6+1=? First right",
                },
                Asked());
        }

        [Test]
        public void ReturnedItem_AnsweredRight_IsPraisedAndTheRoundGoesOn()
        {
            List<Item> items = Items(5);
            ArithmeticPlay play = Play(items);
            EnterWrong(play);
            EnterWrong(play);
            play.Press(AnswerKey.Confirm);
            for (int other = 0; other < 3; other++)
            {
                EnterRight(play);
            }

            Assert.AreSame(items[0], play.CurrentItem);

            Feedback feedback = EnterRight(play);

            Assert.AreEqual(FeedbackKind.Praise, feedback.Kind);
            Assert.AreEqual("A:1+1=? Return right", Asked()[5]);
            Assert.AreSame(items[4], play.CurrentItem);
        }

        [Test]
        public void ResponseTime_RunsFromWhenTheItemWasPutInFrontOfTheChild()
        {
            ArithmeticPlay play = Play(Items(3));

            _clock.Now = Start.AddMilliseconds(1500);
            EnterWrong(play);
            _clock.Now = _clock.Now.AddMilliseconds(700);
            EnterWrong(play);
            _clock.Now = _clock.Now.AddMilliseconds(5000);
            play.Press(AnswerKey.Confirm);
            _clock.Now = _clock.Now.AddMilliseconds(300);
            EnterRight(play);

            int[] times = _log.ReadAll().Select(r => r.ResponseMilliseconds).ToArray();
            Assert.AreEqual(new[] { 1500, 700, 300 }, times);
        }

        [Test]
        public void ResponseTime_AfterAPraisedAnswer_StartsAtThatAnswer()
        {
            ArithmeticPlay play = Play(Items(3));
            _clock.Now = Start.AddMilliseconds(2000);
            EnterRight(play);

            _clock.Now = _clock.Now.AddMilliseconds(900);
            EnterRight(play);

            Assert.AreEqual(new[] { 2000, 900 }, _log.ReadAll().Select(r => r.ResponseMilliseconds).ToArray());
        }

        [Test]
        public void RoundEnd_CountsOnlyItemsRightFirstTime_AndIgnoresFurtherPresses()
        {
            ArithmeticPlay play = Play(Items(2));
            EnterWrong(play);
            EnterRight(play);
            EnterRight(play);
            EnterRight(play);

            Assert.IsTrue(play.IsEnded);
            Assert.IsNull(play.CurrentItem);
            Assert.AreEqual("1", play.SummaryArgs["Right"]);
            Assert.AreEqual("2", play.SummaryArgs["Total"]);

            int records = _log.ReadAll().Count;
            Assert.IsNull(play.Press(AnswerKey.Digit4));
            Assert.IsNull(play.Press(AnswerKey.Confirm));
            Assert.AreEqual(records, _log.ReadAll().Count);
        }
    }
}
