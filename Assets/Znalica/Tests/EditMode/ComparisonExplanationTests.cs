using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    /// <summary>What a wrong comparison answer shows and says, and that the shipped Hr.json can say it.</summary>
    public class ComparisonExplanationTests
    {
        private static PhraseCatalog ShippedCatalog(CollectingProblemLog problems)
        {
            return PhraseCatalogFile.Read(PhraseCatalogFile.PathIn(Application.streamingAssetsPath), problems);
        }

        [Test]
        public void SignExplanationWithNumbers_IsTheTwoNumbersSideBySide_WithTheRelationOfTheRightSign()
        {
            Assert.AreEqual(PhraseKey.ExplainLess, ComparisonItem.ChooseSign(Representation.Numbers, 7, 12).Explain().Phrase);
            Assert.AreEqual(PhraseKey.ExplainMore, ComparisonItem.ChooseSign(Representation.Numbers, 12, 7).Explain().Phrase);
            Assert.AreEqual(PhraseKey.ExplainSame, ComparisonItem.ChooseSign(Representation.Numbers, 7, 7).Explain().Phrase);

            Explanation explanation = ComparisonItem.ChooseSign(Representation.Numbers, 7, 12).Explain();

            Assert.AreEqual(ExplanationKind.SideBySide, explanation.Kind);
            Assert.AreEqual((7, 12, Representation.Numbers), (explanation.Left, explanation.Right, explanation.Representation));
        }

        [Test]
        public void SignExplanationWithGroups_IsHereAndThere_WhateverTheSign()
        {
            foreach ((int left, int right) in new[] { (7, 12), (12, 7), (7, 7) })
            {
                Explanation explanation = ComparisonItem.ChooseSign(Representation.Groups, left, right).Explain();

                Assert.AreEqual(ExplanationKind.SideBySide, explanation.Kind);
                Assert.AreEqual(PhraseKey.ExplainGroups, explanation.Phrase);
                Assert.AreEqual(Representation.Groups, explanation.Representation);
                Assert.AreEqual((left, right), (explanation.Left, explanation.Right));
            }
        }

        [TestCase(new[] { 14, 3, 9 }, PhraseKey.ExplainOrder3)]
        [TestCase(new[] { 14, 3, 9, 1 }, PhraseKey.ExplainOrder4)]
        [TestCase(new[] { 14, 3, 9, 1, 20 }, PhraseKey.ExplainOrder5)]
        public void OrderingExplanation_IsTheNumbersInOrder_WithThePhraseForHowManyThereAre(int[] numbers, PhraseKey phrase)
        {
            Explanation explanation = ComparisonItem.PutInOrder(numbers).Explain();

            Assert.AreEqual(ExplanationKind.OrderedSequence, explanation.Kind);
            Assert.AreEqual(phrase, explanation.Phrase);
            CollectionAssert.AreEqual(numbers.OrderBy(n => n).ToArray(), explanation.Numbers);
        }

        [Test]
        public void ShippedPhrases_FillTheirSlotsInTextAndVoice_ForEveryKindOfComparisonExplanation()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = ShippedCatalog(problems);

            Feedback less = Feedback.Explain(ComparisonItem.ChooseSign(Representation.Numbers, 7, 12).Explain());
            Feedback more = Feedback.Explain(ComparisonItem.ChooseSign(Representation.Numbers, 12, 7).Explain());
            Feedback same = Feedback.Explain(ComparisonItem.ChooseSign(Representation.Numbers, 7, 7).Explain());
            Feedback groups = Feedback.Explain(ComparisonItem.ChooseSign(Representation.Groups, 7, 12).Explain());
            Feedback order = Feedback.Explain(ComparisonItem.PutInOrder(new[] { 14, 3, 9 }).Explain());
            Feedback order5 = Feedback.Explain(ComparisonItem.PutInOrder(new[] { 14, 3, 9, 1, 20 }).Explain());

            Assert.AreEqual("7 je manje od 12.", catalog.Text(less.Phrase, less.Args));
            Assert.AreEqual(new[] { "Number07", "LessThan", "Number12" }, catalog.VoiceClips(less.Phrase, less.Args));
            Assert.AreEqual("12 je veće od 7.", catalog.Text(more.Phrase, more.Args));
            Assert.AreEqual(new[] { "Number12", "MoreThan", "Number07" }, catalog.VoiceClips(more.Phrase, more.Args));
            Assert.AreEqual("7 je jednako 7.", catalog.Text(same.Phrase, same.Args));
            Assert.AreEqual(new[] { "Number07", "SameAs", "Number07" }, catalog.VoiceClips(same.Phrase, same.Args));
            Assert.AreEqual("Ovdje 7, ondje 12.", catalog.Text(groups.Phrase, groups.Args));
            Assert.AreEqual(new[] { "Here", "Number07", "There", "Number12" }, catalog.VoiceClips(groups.Phrase, groups.Args));
            Assert.AreEqual("Redom: 3, 9, 14.", catalog.Text(order.Phrase, order.Args));
            Assert.AreEqual(new[] { "InOrder", "Number03", "Number09", "Number14" }, catalog.VoiceClips(order.Phrase, order.Args));
            Assert.AreEqual("Redom: 1, 3, 9, 14, 20.", catalog.Text(order5.Phrase, order5.Args));
            Assert.AreEqual(
                new[] { "InOrder", "Number01", "Number03", "Number09", "Number14", "Number20" },
                catalog.VoiceClips(order5.Phrase, order5.Args));
            Assert.IsEmpty(problems.Messages);
        }
    }
}
