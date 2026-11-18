using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class SchoolCheckSessionTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 11, 18, 15, 0, 0, TimeSpan.FromHours(1));

        private InMemoryCheckLog _log;
        private FixedClock _clock;

        [SetUp]
        public void SetUp()
        {
            _log = new InMemoryCheckLog();
            _clock = new FixedClock(Now);
        }

        private SchoolCheckSession Planned(int seed = 1)
        {
            return new SchoolCheckSession(SchoolCheckPlanner.Plan(new SeededRandom(seed)), _log, _clock, "check-1");
        }

        private SchoolCheckSession Of(params Item[] items)
        {
            return new SchoolCheckSession(items, _log, _clock, "check-1");
        }

        [Test]
        public void ANumberLineItem_IsScoredFromTheTypedNumber()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14), NumberLineItem.After(12), NumberLineItem.Before(7));
            CheckAnswers.Type(session, 0, 13);
            CheckAnswers.Type(session, 1, 13);
            CheckAnswers.Type(session, 2, 8);

            session.FinishPage();

            CheckLine[] answers = _log.ReadAll().Where(line => !line.IsClosing).ToArray();
            CollectionAssert.AreEqual(new[] { "13", "13", "8" }, answers.Select(line => line.Answer).ToArray());
            CollectionAssert.AreEqual(new[] { true, true, false }, answers.Select(line => line.IsCorrect).ToArray());
            Assert.AreEqual(2, session.Score());
        }

        [Test]
        public void ASignItem_IsScoredFromTheChosenSign()
        {
            SchoolCheckSession session = Of(
                ComparisonItem.ChooseSign(Representation.Numbers, 7, 12), ComparisonItem.ChooseSign(Representation.Numbers, 9, 4));
            Assert.IsTrue(session.Choose(0, Sign.Less));
            Assert.IsTrue(session.Choose(1, Sign.Less));

            session.FinishPage();

            CheckLine[] answers = _log.ReadAll().Where(line => !line.IsClosing).ToArray();
            CollectionAssert.AreEqual(new[] { "Less", "Less" }, answers.Select(line => line.Answer).ToArray());
            CollectionAssert.AreEqual(new[] { true, false }, answers.Select(line => line.IsCorrect).ToArray());
        }

        [Test]
        public void AnItemLeftEmpty_IsRecordedAsAnEmptyWrongAnswer()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14), ComparisonItem.ChooseSign(Representation.Numbers, 7, 12));

            session.FinishPage();

            CheckLine[] answers = _log.ReadAll().Where(line => !line.IsClosing).ToArray();
            Assert.AreEqual(2, answers.Length);
            Assert.IsTrue(answers.All(line => line.Answer == string.Empty && !line.IsCorrect));
        }

        [Test]
        public void NothingIsRecorded_BeforeThePageIsFinished_AndABoxCanStillBeChanged()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14));

            CheckAnswers.Type(session, 0, 5);
            session.Press(0, AnswerKey.Erase);
            CheckAnswers.Type(session, 0, 13);

            Assert.IsEmpty(_log.ReadAll());
            Assert.AreEqual(13, session.Entered(0));
            session.FinishPage();
            Assert.AreEqual("13", _log.ReadAll()[0].Answer);
        }

        [Test]
        public void AFullCheck_HasTwoPagesOfTen_RecordsTwentyAnswersAndOneClosingLineWithTheScore()
        {
            SchoolCheckSession session = Planned();
            Assert.AreEqual(2, session.PageCount);
            Assert.AreEqual(0, session.PageStart);
            Assert.AreEqual(10, session.PageEnd);

            CheckAnswers.FillPageRight(session);
            session.FinishPage();
            Assert.AreEqual(1, session.Page);
            Assert.AreEqual(10, session.PageStart);
            Assert.AreEqual(20, session.PageEnd);
            Assert.AreEqual(10, _log.ReadAll().Count);
            Assert.IsFalse(session.IsFinished);

            CheckAnswers.FillPageRight(session);
            // One answer is changed to a wrong one before the page is finished.
            CheckAnswers.Type(session, session.PageEnd - 1, (((NumberLineItem)session.Items[19]).Solution + 1) % 21);
            session.FinishPage();

            Assert.IsTrue(session.IsFinished);
            CheckLine[] lines = _log.ReadAll().ToArray();
            Assert.AreEqual(21, lines.Length);
            Assert.AreEqual(20, lines.Count(line => !line.IsClosing));
            CheckLine closing = lines.Last();
            Assert.IsTrue(closing.IsClosing);
            Assert.AreEqual(19, closing.Score);
            Assert.AreEqual(20, closing.ItemCount);
            Assert.AreEqual("check-1", closing.CheckId);
            CollectionAssert.AreEqual(session.Items.Select(item => item.Key.ToString()).ToArray(), lines.Take(20).Select(line => line.ItemKey).ToArray());
        }

        [Test]
        public void ACheckLeftAfterThePageOne_HasNoClosingLine_IsNotFinished_AndHasNoScore()
        {
            SchoolCheckSession session = Planned();
            CheckAnswers.FillPageRight(session);
            session.FinishPage();

            CheckResult result = CheckHistory.From(_log.ReadAll()).Single();

            Assert.IsFalse(session.IsFinished);
            Assert.IsFalse(result.IsFinished);
            Assert.IsNull(result.Score);
            Assert.IsNull(result.ItemCount);
            Assert.AreEqual(10, result.Answers.Count);
        }

        [Test]
        public void ADigitThatWouldPassTwenty_IsRefused_AsInTheGame()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14));
            Assert.IsTrue(session.Press(0, AnswerKey.Digit2));

            Assert.IsFalse(session.Press(0, AnswerKey.Digit5));
            Assert.IsTrue(session.Press(0, AnswerKey.Digit0));
            Assert.AreEqual(20, session.Entered(0));
            Assert.IsFalse(session.Press(0, AnswerKey.Digit1));
        }

        [Test]
        public void KeysThatAreNotDigitsOrErase_AndItemsThatTakeNoNumber_AreLeftAlone()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14), ComparisonItem.ChooseSign(Representation.Numbers, 7, 12));

            Assert.IsFalse(session.Press(0, AnswerKey.Confirm));
            Assert.IsFalse(session.Press(0, AnswerKey.Continue));
            Assert.IsFalse(session.Press(0, AnswerKey.Erase), "Erasing an empty box changes nothing.");
            Assert.IsFalse(session.Press(1, AnswerKey.Digit3));
            Assert.IsFalse(session.Choose(0, Sign.Less));
            Assert.IsNull(session.Entered(0));
            Assert.IsNull(session.Entered(1));
            Assert.IsNull(session.Chosen(1));
        }

        [Test]
        public void AnItemOnAnotherPage_CannotBeChanged()
        {
            SchoolCheckSession session = Planned();

            Assert.IsFalse(session.Press(15, AnswerKey.Digit1));
            Assert.IsNull(session.Entered(15));
            Assert.IsFalse(session.Choose(12, Sign.Less));
        }

        [Test]
        public void WhenTheLogFailsPartWay_ThePageCanBeFinishedAgain_WithoutWritingAnyLineTwice()
        {
            SchoolCheckSession session = Planned();
            CheckAnswers.FillPageRight(session);
            _log.FailOnAppend = 4;

            Assert.Throws<IOException>(session.FinishPage);
            Assert.AreEqual(3, _log.ReadAll().Count);
            Assert.AreEqual(0, session.Page, "The page is not left until all of it is recorded.");

            session.FinishPage();

            Assert.AreEqual(1, session.Page);
            CheckLine[] lines = _log.ReadAll().ToArray();
            Assert.AreEqual(10, lines.Length);
            Assert.AreEqual(10, lines.Select(line => line.ItemKey).Distinct().Count());
        }

        [Test]
        public void WhenTheClosingLineFails_FinishingAgainWritesOnlyIt()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14));
            CheckAnswers.FillPageRight(session);
            _log.FailOnAppend = 2;

            Assert.Throws<IOException>(session.FinishPage);
            Assert.IsFalse(session.IsFinished);
            session.FinishPage();

            Assert.IsTrue(session.IsFinished);
            Assert.AreEqual(2, _log.ReadAll().Count);
            Assert.IsTrue(_log.ReadAll()[1].IsClosing);
        }

        [Test]
        public void AFinishedCheck_RefusesFurtherAnswersAndAFurtherFinish()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14));
            session.FinishPage();

            Assert.IsFalse(session.Press(0, AnswerKey.Digit1));
            Assert.Throws<InvalidOperationException>(session.FinishPage);
            Assert.AreEqual(2, _log.ReadAll().Count);
        }

        [Test]
        public void TheAnswerTimeInTheLog_IsTheClocksTime()
        {
            SchoolCheckSession session = Of(NumberLineItem.Before(14));

            session.FinishPage();

            Assert.AreEqual(Now.ToString("o"), _log.ReadAll()[0].AnsweredAt);
            Assert.AreEqual(Now.ToString("o"), _log.ReadAll()[1].CompletedAt);
        }

        [Test]
        public void ASessionWithAnItemTheWorksheetHasNoPlaceFor_IsRefused()
        {
            Assert.Throws<ArgumentException>(() => Of(NumberLineItem.Place(4)));
        }
    }
}
