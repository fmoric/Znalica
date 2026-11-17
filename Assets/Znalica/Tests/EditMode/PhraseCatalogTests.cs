using System.Collections.Generic;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class PhraseCatalogTests
    {
        private CollectingProblemLog _problems;

        [SetUp]
        public void SetUp()
        {
            _problems = new CollectingProblemLog();
        }

        private PhraseCatalog CatalogWith(params (PhraseKey Key, string Text, string[] Voice)[] entries)
        {
            var list = new List<KeyValuePair<PhraseKey, Phrase>>();
            foreach (var entry in entries)
            {
                list.Add(new KeyValuePair<PhraseKey, Phrase>(entry.Key, new Phrase(entry.Text, entry.Voice)));
            }

            return new PhraseCatalog(list, _problems);
        }

        private PhraseCatalog ExplainJumpCatalog()
        {
            return CatalogWith((
                PhraseKey.ExplainJump,
                "{Start}, pa još {Change}, to je {End}.",
                new[] { "{Start}", "ThenMore", "{Change}", "Is", "{End}" }));
        }

        [Test]
        public void Get_KeyWithoutEntry_GivesTheKeyNameAndReportsIt()
        {
            PhraseCatalog catalog = CatalogWith();

            Phrase phrase = catalog.Get(PhraseKey.PraiseRight);

            Assert.AreEqual("PraiseRight", phrase.Text);
            Assert.IsEmpty(phrase.Voice);
            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("PraiseRight", _problems.Messages[0]);
        }

        [Test]
        public void Get_KeyWithEntry_GivesItsTextAndVoiceAndReportsNothing()
        {
            PhraseCatalog catalog = CatalogWith((PhraseKey.PraiseRight, "Bravo!", new[] { "PraiseRight" }));

            Phrase phrase = catalog.Get(PhraseKey.PraiseRight);

            Assert.AreEqual("Bravo!", phrase.Text);
            CollectionAssert.AreEqual(new[] { "PraiseRight" }, phrase.Voice);
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void Text_FillsEachSlotFromItsArgument()
        {
            PhraseCatalog catalog = ExplainJumpCatalog();
            var args = new Dictionary<string, string> { ["Start"] = "8", ["Change"] = "5", ["End"] = "13" };

            Assert.AreEqual("8, pa još 5, to je 13.", catalog.Text(PhraseKey.ExplainJump, args));
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void Text_SlotWithoutArgument_IsReportedAndLeftInPlace()
        {
            PhraseCatalog catalog = ExplainJumpCatalog();
            var args = new Dictionary<string, string> { ["Start"] = "8", ["End"] = "13" };

            Assert.AreEqual("8, pa još {Change}, to je 13.", catalog.Text(PhraseKey.ExplainJump, args));
            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("Change", _problems.Messages[0]);
        }

        [Test]
        public void VoiceClips_NumberSlotsBecomeBareNumeralClipsBetweenTheFixedClips()
        {
            PhraseCatalog catalog = ExplainJumpCatalog();
            var args = new Dictionary<string, string> { ["Start"] = "8", ["Change"] = "5", ["End"] = "13" };

            IReadOnlyList<string> clips = catalog.VoiceClips(PhraseKey.ExplainJump, args);

            CollectionAssert.AreEqual(new[] { "Number08", "ThenMore", "Number05", "Is", "Number13" }, clips);
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void VoiceClips_NumberOutsideZeroToTwenty_IsReportedAndLeftOut()
        {
            PhraseCatalog catalog = ExplainJumpCatalog();
            var args = new Dictionary<string, string> { ["Start"] = "8", ["Change"] = "5", ["End"] = "21" };

            IReadOnlyList<string> clips = catalog.VoiceClips(PhraseKey.ExplainJump, args);

            CollectionAssert.AreEqual(new[] { "Number08", "ThenMore", "Number05", "Is" }, clips);
            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("End", _problems.Messages[0]);
        }

        [TestCase("0", "Number00")]
        [TestCase("8", "Number08")]
        [TestCase("08", "Number08")]
        [TestCase("20", "Number20")]
        public void VoiceClips_NumberInsideZeroToTwenty_IsAccepted(string value, string expectedClip)
        {
            PhraseCatalog catalog = CatalogWith((PhraseKey.ThenMore, "x", new[] { "{N}" }));

            IReadOnlyList<string> clips = catalog.VoiceClips(PhraseKey.ThenMore, new Dictionary<string, string> { ["N"] = value });

            CollectionAssert.AreEqual(new[] { expectedClip }, clips);
            Assert.IsEmpty(_problems.Messages);
        }

        [TestCase("-1")]
        [TestCase("21")]
        [TestCase("+8")]
        public void VoiceClips_NumberOutsideTheRangeOrNotADigitString_IsRejected(string value)
        {
            PhraseCatalog catalog = CatalogWith((PhraseKey.ThenMore, "x", new[] { "{N}" }));

            IReadOnlyList<string> clips = catalog.VoiceClips(PhraseKey.ThenMore, new Dictionary<string, string> { ["N"] = value });

            Assert.IsEmpty(clips);
            Assert.AreEqual(1, _problems.Messages.Count);
        }

        [Test]
        public void VoiceClips_SlotWithoutArgumentOrWithNonNumber_IsReportedAndLeftOut()
        {
            PhraseCatalog catalog = ExplainJumpCatalog();
            var args = new Dictionary<string, string> { ["Start"] = "osam", ["Change"] = "5" };

            IReadOnlyList<string> clips = catalog.VoiceClips(PhraseKey.ExplainJump, args);

            CollectionAssert.AreEqual(new[] { "ThenMore", "Number05", "Is" }, clips);
            Assert.AreEqual(2, _problems.Messages.Count);
        }

        [Test]
        public void VoiceClips_PhraseWithoutNumbers_IsItsOneWholeSentenceClip()
        {
            PhraseCatalog catalog = CatalogWith((PhraseKey.PraiseRight, "Bravo!", new[] { "PraiseRight" }));

            CollectionAssert.AreEqual(new[] { "PraiseRight" }, catalog.VoiceClips(PhraseKey.PraiseRight));
        }

        [Test]
        public void VoiceClips_ScreenOnlyPhrase_HasNoClips()
        {
            PhraseCatalog catalog = CatalogWith((PhraseKey.ButtonNext, "Dalje", new string[0]));

            Assert.IsEmpty(catalog.VoiceClips(PhraseKey.ButtonNext));
        }
    }
}
