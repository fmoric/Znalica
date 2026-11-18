using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    /// <summary>What a wrong number-line answer shows and says, how a number-line task is shown, how it is planned, and that the shipped Hr.json can say it.</summary>
    public class NumberLineExplanationTests
    {
        private static PhraseCatalog ShippedCatalog(CollectingProblemLog problems)
        {
            return PhraseCatalogFile.Read(PhraseCatalogFile.PathIn(Application.streamingAssetsPath), problems);
        }

        [Test]
        public void PlacingExplanation_IsTheRightPlace()
        {
            Explanation explanation = NumberLineItem.Place(14).Explain();

            Assert.AreEqual(ExplanationKind.PositionOnLine, explanation.Kind);
            Assert.AreEqual(PhraseKey.ExplainPlace, explanation.Phrase);
            Assert.AreEqual((14, 14), (explanation.Target, explanation.Given));
        }

        [Test]
        public void BeforeAndAfterExplanations_MarkTheNeighbour_AndNameTheNumberTheyWereAskedFrom()
        {
            Explanation before = NumberLineItem.Before(14).Explain();
            Explanation after = NumberLineItem.After(14).Explain();

            Assert.AreEqual((ExplanationKind.PositionOnLine, PhraseKey.ExplainBefore, 13, 14), (before.Kind, before.Phrase, before.Target, before.Given));
            Assert.AreEqual((ExplanationKind.PositionOnLine, PhraseKey.ExplainAfter, 15, 14), (after.Kind, after.Phrase, after.Target, after.Given));
        }

        [Test]
        public void ShippedPhrases_FillTheirSlotsInTextAndVoice_ForEveryKindOfNumberLineExplanation()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = ShippedCatalog(problems);

            Feedback place = Feedback.Explain(NumberLineItem.Place(14).Explain());
            Feedback before = Feedback.Explain(NumberLineItem.Before(14).Explain());
            Feedback after = Feedback.Explain(NumberLineItem.After(14).Explain());
            Feedback jump = Feedback.Explain(NumberLineItem.Jump(6, 3).Explain());
            Feedback back = Feedback.Explain(NumberLineItem.Jump(9, -3).Explain());

            Assert.AreEqual("Ovdje 14.", catalog.Text(place.Phrase, place.Args));
            Assert.AreEqual(new[] { "Here", "Number14" }, catalog.VoiceClips(place.Phrase, place.Args));
            Assert.AreEqual("Ispred 14, to je 13.", catalog.Text(before.Phrase, before.Args));
            Assert.AreEqual(new[] { "InFrontOf", "Number14", "Is", "Number13" }, catalog.VoiceClips(before.Phrase, before.Args));
            Assert.AreEqual("Iza 14, to je 15.", catalog.Text(after.Phrase, after.Args));
            Assert.AreEqual(new[] { "Behind", "Number14", "Is", "Number15" }, catalog.VoiceClips(after.Phrase, after.Args));
            Assert.AreEqual("6, pa još 3, to je 9.", catalog.Text(jump.Phrase, jump.Args));
            Assert.AreEqual("9, pa manje za 3, to je 6.", catalog.Text(back.Phrase, back.Args));
            Assert.IsEmpty(problems.Messages);
        }

        [Test]
        public void Tokens_ShowTheTaskAsTheChildSeesIt()
        {
            Assert.AreEqual(new[] { "14" }, Texts(NumberLineItem.Place(14)));
            Assert.AreEqual(new[] { "?", ",", "14" }, Texts(NumberLineItem.Before(14)));
            Assert.AreEqual(new[] { "14", ",", "?" }, Texts(NumberLineItem.After(14)));
            Assert.AreEqual(new[] { "6", "+3", "=", "?" }, Texts(NumberLineItem.Jump(6, 3)));
            Assert.AreEqual(new[] { "9", ItemFormatter.MinusSign + "3", "=", "?" }, Texts(NumberLineItem.Jump(9, -3)));
            Assert.AreEqual(new[] { false, false, true }, ItemFormatter.Tokens(NumberLineItem.After(14)).Select(t => t.IsUnknown).ToArray());
            Assert.AreEqual(new[] { false }, ItemFormatter.Tokens(NumberLineItem.Place(14)).Select(t => t.IsUnknown).ToArray(), "A number to place has no box to fill.");
        }

        [Test]
        public void Plan_FromTheNumberLineSet_NeedsNoPlannerOfItsOwn_AndBringsAMissedJumpBack()
        {
            var set = new NumberLineItemSet();
            var schedule = new ItemSchedule();
            schedule.Apply(TestRecords.Make(sessionNumber: 1, itemKey: "L:Jump:6+3", answer: "8", isCorrect: false));
            schedule.Apply(TestRecords.Make(sessionNumber: 1, itemKey: "L:Place:14", answer: "14", isCorrect: true));

            System.Collections.Generic.IReadOnlyList<Item> round = RoundPlanner.Plan(set, schedule, 2, new SeededRandom(3));

            Assert.AreEqual(RoundPlanner.RoundSize, round.Count);
            Assert.AreEqual(round.Count, round.Select(i => i.Key.ToString()).Distinct().Count());
            Assert.IsTrue(round.All(i => i.Activity == Activity.NumberLine));
            Assert.IsTrue(round.Any(i => i.Key.ToString() == "L:Jump:6+3"), "The jump missed in session 1 is due in session 2.");
            Assert.IsTrue(round.Any(i => i.Key.ToString() == "L:Place:14"), "A box 1 item is due the session after.");
            Assert.IsTrue(round.Where(i => i.Key.ToString() != "L:Jump:6+3" && i.Key.ToString() != "L:Place:14")
                .All(i => i.Difficulty == Difficulty.WithinTen), "New items come from the easiest level.");
        }

        private static string[] Texts(Item item)
        {
            return ItemFormatter.Tokens(item).Select(t => t.Text).ToArray();
        }
    }
}
