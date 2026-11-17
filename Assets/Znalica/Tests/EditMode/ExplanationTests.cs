using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    public class ExplanationTests
    {
        private static PhraseCatalog ShippedCatalog(CollectingProblemLog problems)
        {
            return PhraseCatalogFile.Read(PhraseCatalogFile.PathIn(Application.streamingAssetsPath), problems);
        }

        [Test]
        public void AdditionExplanation_IsTheJumpUpFromTheFirstNumber()
        {
            Explanation explanation = new ArithmeticItem(8, Operation.Addition, 5, 13, Unknown.First).Explain();

            Assert.AreEqual(ExplanationKind.NumberLineJump, explanation.Kind);
            Assert.AreEqual(PhraseKey.ExplainJump, explanation.Phrase);
            Assert.AreEqual((8, 5, 13), (explanation.Start, explanation.Change, explanation.End));
        }

        [Test]
        public void SubtractionExplanation_IsTheJumpBackFromTheFirstNumber()
        {
            Explanation explanation = new ArithmeticItem(13, Operation.Subtraction, 6, 7, Unknown.Second).Explain();

            Assert.AreEqual(PhraseKey.ExplainJumpBack, explanation.Phrase);
            Assert.AreEqual((13, -6, 7), (explanation.Start, explanation.Change, explanation.End));
        }

        [Test]
        public void Explanation_ShowsTheWholeFact_WhicheverNumberWasMissing()
        {
            foreach (Unknown unknown in new[] { Unknown.Result, Unknown.First, Unknown.Second })
            {
                Explanation explanation = new ArithmeticItem(8, Operation.Addition, 5, 13, unknown).Explain();

                Assert.AreEqual((8, 5, 13), (explanation.Start, explanation.Change, explanation.End), unknown.ToString());
            }
        }

        [Test]
        public void PhraseArgs_GiveTheSizeOfTheJumpWithoutItsSign()
        {
            IReadOnlyDictionary<string, string> args = new ArithmeticItem(13, Operation.Subtraction, 6, 7, Unknown.Result).Explain().PhraseArgs;

            Assert.AreEqual("13", args["Start"]);
            Assert.AreEqual("6", args["Change"]);
            Assert.AreEqual("7", args["End"]);
        }

        [Test]
        public void AdditionExplanation_SpeaksAndShowsThroughTheShippedCatalog()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = ShippedCatalog(problems);
            Explanation explanation = new ArithmeticItem(8, Operation.Addition, 5, 13, Unknown.Result).Explain();

            CollectionAssert.AreEqual(
                new[] { "Number08", "ThenMore", "Number05", "Is", "Number13" },
                catalog.VoiceClips(explanation.Phrase, explanation.PhraseArgs));
            Assert.AreEqual("8, pa još 5, to je 13.", catalog.Text(explanation.Phrase, explanation.PhraseArgs));
            Assert.IsEmpty(problems.Messages);
        }

        [Test]
        public void SubtractionExplanation_SpeaksAndShowsThroughTheShippedCatalog()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = ShippedCatalog(problems);
            Explanation explanation = new ArithmeticItem(13, Operation.Subtraction, 6, 7, Unknown.Result).Explain();

            CollectionAssert.AreEqual(
                new[] { "Number13", "ThenLess", "Number06", "Is", "Number07" },
                catalog.VoiceClips(explanation.Phrase, explanation.PhraseArgs));
            Assert.AreEqual("13, pa manje za 6, to je 7.", catalog.Text(explanation.Phrase, explanation.PhraseArgs));
            Assert.IsEmpty(problems.Messages);
        }

        [Test]
        public void EveryExplanationOfTheArithmeticSet_FillsEverySlotOfItsPhrase()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = ShippedCatalog(problems);

            foreach (Item item in new ArithmeticItemSet().All)
            {
                Explanation explanation = item.Explain();
                Assert.AreEqual(5, catalog.VoiceClips(explanation.Phrase, explanation.PhraseArgs).Count, item.Key.ToString());
            }

            Assert.IsEmpty(problems.Messages);
        }
    }
}
