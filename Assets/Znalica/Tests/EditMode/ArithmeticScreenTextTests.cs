using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    /// <summary>The shipped Hr.json against what the arithmetic screen asks of it: its keys, and the slots the play fills.</summary>
    public class ArithmeticScreenTextTests
    {
        private static PhraseCatalog ShippedCatalog(CollectingProblemLog problems)
        {
            return PhraseCatalogFile.Read(PhraseCatalogFile.PathIn(Application.streamingAssetsPath), problems);
        }

        [Test]
        public void RoundEndScore_IsFilledFromTheSummaryTheScreenGives()
        {
            var problems = new CollectingProblemLog();
            var log = new InMemoryAnswerLog();
            var clock = new FixedClock(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
            var items = new List<Item>
            {
                new ArithmeticItem(1, Operation.Addition, 1, 2, Unknown.Result),
                new ArithmeticItem(2, Operation.Addition, 1, 3, Unknown.Result),
                new ArithmeticItem(3, Operation.Addition, 1, 4, Unknown.Result),
            };
            var play = new ArithmeticPlay(new RoundSession(items, log, clock, 1, "r"), clock);
            play.Press(AnswerKey.Digit2);
            play.Press(AnswerKey.Confirm);
            play.Press(AnswerKey.Digit9);
            play.Press(AnswerKey.Confirm);
            play.Press(AnswerKey.Digit3);
            play.Press(AnswerKey.Confirm);

            string text = ShippedCatalog(problems).Text(PhraseKey.RoundEndScore, play.SummaryArgs);

            Assert.AreEqual("1 / 3", text);
            Assert.IsEmpty(problems.Messages);
        }

        [Test]
        public void ExplanationFeedback_FillsTheSlotsOfItsPhraseInTextAndVoice()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = ShippedCatalog(problems);
            Feedback feedback = Feedback.Explain(new ArithmeticItem(8, Operation.Addition, 5, 13, Unknown.Result).Explain());

            Assert.AreEqual("8, pa još 5, to je 13.", catalog.Text(feedback.Phrase, feedback.Args));
            Assert.AreEqual(
                new[] { "Number08", "ThenMore", "Number05", "Is", "Number13" },
                catalog.VoiceClips(feedback.Phrase, feedback.Args));
            Assert.IsEmpty(problems.Messages);
        }

        [Test]
        public void PraiseFeedback_SpeaksItsPhrase()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = ShippedCatalog(problems);
            Feedback feedback = Feedback.Praise(PhraseKey.PraiseRight);

            Assert.AreEqual(new[] { "PraiseRight" }, catalog.VoiceClips(feedback.Phrase, feedback.Args));
            Assert.IsEmpty(problems.Messages);
        }

        [TestCase(PhraseKey.ButtonErase, "Obriši")]
        [TestCase(PhraseKey.ButtonOk, "U redu")]
        public void PadButtonTexts_AreTheWordsTheSpecGives(PhraseKey key, string text)
        {
            Assert.AreEqual(text, ShippedCatalog(new CollectingProblemLog()).Text(key));
        }
    }
}
