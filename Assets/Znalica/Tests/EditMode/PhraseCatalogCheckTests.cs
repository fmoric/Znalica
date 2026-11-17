using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class PhraseCatalogCheckTests
    {
        /// <summary>A catalog with an entry for every key; each key speaks the clip of its own name.</summary>
        private static List<KeyValuePair<PhraseKey, Phrase>> EveryKey()
        {
            return Enum.GetValues(typeof(PhraseKey)).Cast<PhraseKey>()
                .Select(key => new KeyValuePair<PhraseKey, Phrase>(key, new Phrase("Tekst", new[] { key.ToString() })))
                .ToList();
        }

        private static IEnumerable<string> AllKeyClips()
        {
            return Enum.GetNames(typeof(PhraseKey));
        }

        private static void Replace(List<KeyValuePair<PhraseKey, Phrase>> entries, PhraseKey key, Phrase phrase)
        {
            entries[entries.FindIndex(entry => entry.Key == key)] = new KeyValuePair<PhraseKey, Phrase>(key, phrase);
        }

        private static IReadOnlyList<string> Check(List<KeyValuePair<PhraseKey, Phrase>> entries, IEnumerable<string> clips)
        {
            return PhraseCatalogCheck.Missing(new PhraseCatalog(entries, new CollectingProblemLog()), clips);
        }

        [Test]
        public void Missing_EveryKeyHasTextAndEveryClipExists_IsEmpty()
        {
            Assert.IsEmpty(Check(EveryKey(), AllKeyClips()));
        }

        [Test]
        public void Missing_KeyWithoutEntry_IsNamed()
        {
            List<KeyValuePair<PhraseKey, Phrase>> entries = EveryKey();
            entries.RemoveAll(entry => entry.Key == PhraseKey.PraiseRight);

            IReadOnlyList<string> problems = Check(entries, AllKeyClips());

            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains("PraiseRight", problems[0]);
        }

        [Test]
        public void Missing_KeyWithEmptyText_IsNamed()
        {
            List<KeyValuePair<PhraseKey, Phrase>> entries = EveryKey();
            Replace(entries, PhraseKey.ButtonNext, new Phrase("  ", new string[0]));

            IReadOnlyList<string> problems = Check(entries, AllKeyClips());

            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains("ButtonNext", problems[0]);
        }

        [Test]
        public void Missing_ClipNotInTheFolder_IsNamedWithItsPhrase()
        {
            IEnumerable<string> clips = AllKeyClips().Where(name => name != "PraiseRight");

            IReadOnlyList<string> problems = Check(EveryKey(), clips);

            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains("PraiseRight", problems[0]);
        }

        [Test]
        public void Missing_ClipNameDiffersOnlyInCase_CountsAsMissing()
        {
            IEnumerable<string> clips = AllKeyClips().Select(name => name == "PraiseRight" ? "praiseright" : name);

            Assert.AreEqual(1, Check(EveryKey(), clips).Count);
        }

        [Test]
        public void Missing_ScreenOnlyPhrase_NeedsNoClip()
        {
            List<KeyValuePair<PhraseKey, Phrase>> entries = EveryKey();
            Replace(entries, PhraseKey.ButtonNext, new Phrase("Dalje", new string[0]));
            IEnumerable<string> clips = AllKeyClips().Where(name => name != "ButtonNext");

            Assert.IsEmpty(Check(entries, clips));
        }

        [Test]
        public void Missing_NumberSlot_NeedsEveryClipFromNumber00ToNumber20()
        {
            List<KeyValuePair<PhraseKey, Phrase>> entries = EveryKey();
            Replace(entries, PhraseKey.ExplainJump, new Phrase("{Start}", new[] { "{Start}" }));
            IEnumerable<string> clips = AllKeyClips().Where(name => name != "ExplainJump" && name != "Number20");

            IReadOnlyList<string> problems = Check(entries, clips);

            Assert.AreEqual(2, problems.Count, "Number20 is missing for its own key and for the slot.");
            Assert.IsTrue(problems.All(problem => problem.Contains("Number20")));
        }
    }
}
