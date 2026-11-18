using System;
using System.Collections.Generic;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ParentGateTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 11, 17, 10, 30, 0, TimeSpan.FromHours(1));

        private FixedClock _clock;
        private ParentGate _gate;

        [SetUp]
        public void SetUp()
        {
            _clock = new FixedClock(Now);
            _gate = new ParentGate(_clock, new SeededRandom(7));
        }

        private GateResult Answer(int number)
        {
            foreach (char digit in number.ToString())
            {
                _gate.Press(AnswerKey.Digit0 + (digit - '0'));
            }

            return _gate.Press(AnswerKey.Confirm);
        }

        private int WrongAnswer()
        {
            return _gate.Challenge.Answer + 1;
        }

        [Test]
        public void Next_AlwaysAsksTwoFactorsFromThreeToNine()
        {
            var random = new SeededRandom(1);
            for (int i = 0; i < 500; i++)
            {
                GateChallenge challenge = ParentGate.Next(random);

                Assert.That(challenge.First, Is.InRange(3, 9));
                Assert.That(challenge.Second, Is.InRange(3, 9));
                Assert.AreEqual(challenge.First * challenge.Second, challenge.Answer);
            }
        }

        [Test]
        public void Next_OverManyDraws_ReachesBothEndsOfTheRangeForBothFactors()
        {
            var random = new SeededRandom(2);
            var first = new HashSet<int>();
            var second = new HashSet<int>();
            for (int i = 0; i < 500; i++)
            {
                GateChallenge challenge = ParentGate.Next(random);
                first.Add(challenge.First);
                second.Add(challenge.Second);
            }

            CollectionAssert.AreEquivalent(new[] { 3, 4, 5, 6, 7, 8, 9 }, first);
            CollectionAssert.AreEquivalent(new[] { 3, 4, 5, 6, 7, 8, 9 }, second);
        }

        [Test]
        public void Check_RightProductPasses_AnyOtherNumberDoesNot()
        {
            var challenge = new GateChallenge(6, 7);

            Assert.IsTrue(ParentGate.Check(challenge, 42));
            Assert.IsFalse(ParentGate.Check(challenge, 41));
            Assert.IsFalse(ParentGate.Check(challenge, 13));
        }

        [Test]
        public void Press_RightAnswer_Passes()
        {
            Assert.AreEqual(GateResult.Passed, Answer(_gate.Challenge.Answer));
        }

        [Test]
        public void Press_WrongAnswer_AsksAgainWithoutWaiting()
        {
            Assert.AreEqual(GateResult.Wrong, Answer(WrongAnswer()));

            Assert.IsFalse(_gate.IsWaiting);
            Assert.IsFalse(_gate.Entered.HasValue);
        }

        [Test]
        public void Press_ThreeWrongInARow_MakesTheGateWaitThirtySeconds()
        {
            Assert.AreEqual(GateResult.Wrong, Answer(WrongAnswer()));
            Assert.AreEqual(GateResult.Wrong, Answer(WrongAnswer()));
            Assert.IsFalse(_gate.IsWaiting);

            Assert.AreEqual(GateResult.Locked, Answer(WrongAnswer()));

            Assert.IsTrue(_gate.IsWaiting);
            Assert.AreEqual(30, _gate.SecondsLeft);
        }

        [Test]
        public void Press_DuringTheWait_RefusesEvenTheRightAnswer()
        {
            Answer(WrongAnswer());
            Answer(WrongAnswer());
            Answer(WrongAnswer());
            _clock.Now = Now.AddSeconds(29);

            Assert.IsTrue(_gate.IsWaiting);
            Assert.AreEqual(1, _gate.SecondsLeft);
            Assert.AreEqual(GateResult.Locked, Answer(_gate.Challenge.Answer));
            Assert.IsFalse(_gate.Entered.HasValue);
        }

        [Test]
        public void Press_AfterThirtySeconds_TakesAnswersAgain()
        {
            Answer(WrongAnswer());
            Answer(WrongAnswer());
            Answer(WrongAnswer());

            _clock.Now = Now.AddSeconds(30);

            Assert.IsFalse(_gate.IsWaiting);
            Assert.AreEqual(0, _gate.SecondsLeft);
            Assert.AreEqual(GateResult.Passed, Answer(_gate.Challenge.Answer));
        }

        [Test]
        public void Press_AfterTheWait_ThreeMoreWrongAnswersWaitAgain()
        {
            Answer(WrongAnswer());
            Answer(WrongAnswer());
            Answer(WrongAnswer());
            _clock.Now = Now.AddSeconds(30);

            Assert.AreEqual(GateResult.Wrong, Answer(WrongAnswer()));
            Assert.AreEqual(GateResult.Wrong, Answer(WrongAnswer()));
            Assert.AreEqual(GateResult.Locked, Answer(WrongAnswer()));
        }

        [Test]
        public void Press_TwoWrongThenRightThenTwoWrong_DoesNotWait()
        {
            Answer(WrongAnswer());
            Answer(WrongAnswer());
            Assert.AreEqual(GateResult.Passed, Answer(_gate.Challenge.Answer));

            Assert.AreEqual(GateResult.Wrong, Answer(WrongAnswer()));
            Assert.AreEqual(GateResult.Wrong, Answer(WrongAnswer()));
            Assert.IsFalse(_gate.IsWaiting);
        }

        [Test]
        public void Press_Digits_FillTheBoxUpToTwoDigitsAndRefuseAThird()
        {
            _gate.Press(AnswerKey.Digit8);
            _gate.Press(AnswerKey.Digit1);
            _gate.Press(AnswerKey.Digit9);

            Assert.AreEqual(81, _gate.Entered);
        }

        [Test]
        public void Press_DigitsThatWouldPassEightyOne_AreRefused()
        {
            _gate.Press(AnswerKey.Digit9);
            _gate.Press(AnswerKey.Digit9);

            Assert.AreEqual(9, _gate.Entered);
        }

        [Test]
        public void Press_ConfirmOnAnEmptyBox_DoesNotCountAsAWrongAnswer()
        {
            Assert.AreEqual(GateResult.Typing, _gate.Press(AnswerKey.Confirm));
            Assert.AreEqual(GateResult.Typing, _gate.Press(AnswerKey.Confirm));
            Assert.AreEqual(GateResult.Typing, _gate.Press(AnswerKey.Confirm));

            Assert.IsFalse(_gate.IsWaiting);
        }

        [Test]
        public void Press_Erase_RemovesTheLastDigit()
        {
            _gate.Press(AnswerKey.Digit4);
            _gate.Press(AnswerKey.Digit2);
            _gate.Press(AnswerKey.Erase);

            Assert.AreEqual(4, _gate.Entered);
        }

        [Test]
        public void Text_AsksInWordsNotDigits()
        {
            PhraseCatalog catalog = TestCatalog();

            string text = new GateChallenge(6, 7).Text(catalog);

            Assert.AreEqual("Koliko je šest puta sedam?", text);
        }

        private static PhraseCatalog TestCatalog()
        {
            var entries = new List<KeyValuePair<PhraseKey, Phrase>>
            {
                new KeyValuePair<PhraseKey, Phrase>(PhraseKey.GateQuestion, new Phrase("Koliko je {First} puta {Second}?", new string[0])),
            };
            string[] words = { "tri", "četiri", "pet", "šest", "sedam", "osam", "devet" };
            for (int i = 0; i < words.Length; i++)
            {
                entries.Add(new KeyValuePair<PhraseKey, Phrase>(PhraseKey.NumberWord3 + i, new Phrase(words[i], new string[0])));
            }

            return new PhraseCatalog(entries, new CollectingProblemLog());
        }

        [Test]
        public void ClearEntry_EmptiesTheBox_AndKeepsTheTriesAndTheWait()
        {
            Answer(WrongAnswer());
            Answer(WrongAnswer());
            _gate.Press(AnswerKey.Digit4);
            Assert.IsTrue(_gate.Entered.HasValue);

            _gate.ClearEntry();

            Assert.IsFalse(_gate.Entered.HasValue);
            Assert.AreEqual(GateResult.Locked, Answer(WrongAnswer()), "the two wrong answers still count: the third starts the wait");
        }
    }
}
