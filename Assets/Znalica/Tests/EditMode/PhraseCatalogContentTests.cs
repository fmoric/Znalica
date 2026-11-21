using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    /// <summary>The real Hr.json against the real voice folder: the completeness test of step 2.</summary>
    public class PhraseCatalogContentTests
    {
        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        private static IEnumerable<string> ClipNamesInVoiceFolder()
        {
            string folder = Path.Combine(ProjectRoot, VoiceClipFiles.Folder);
            return Directory.GetFiles(folder, "*" + VoiceClipFiles.Extension)
                .Select(Path.GetFileNameWithoutExtension);
        }

        [Test]
        public void HrJson_ReadsWithoutProblems()
        {
            var problems = new CollectingProblemLog();

            PhraseCatalogFile.Read(PhraseCatalogFile.PathIn(Application.streamingAssetsPath), problems);

            Assert.IsEmpty(problems.Messages);
        }

        [TestCase(PhraseKey.ActivityNameNumberLine)]
        [TestCase(PhraseKey.ActivityNameComparison)]
        [TestCase(PhraseKey.ActivityNameArithmetic)]
        [TestCase(PhraseKey.PraiseRight)]
        [TestCase(PhraseKey.ExplainJump)]
        [TestCase(PhraseKey.ExplainJumpBack)]
        [TestCase(PhraseKey.ExplainLess)]
        [TestCase(PhraseKey.ExplainMore)]
        [TestCase(PhraseKey.ExplainSame)]
        [TestCase(PhraseKey.ExplainGroups)]
        [TestCase(PhraseKey.ExplainOrder3)]
        [TestCase(PhraseKey.ExplainOrder4)]
        [TestCase(PhraseKey.ExplainOrder5)]
        [TestCase(PhraseKey.ExplainPlace)]
        [TestCase(PhraseKey.ExplainBefore)]
        [TestCase(PhraseKey.ExplainAfter)]
        [TestCase(PhraseKey.BreakSuggestion)]
        public void PhraseThatMustSpeak_HasVoiceInTheShippedHrJson(PhraseKey key)
        {
            PhraseCatalog catalog = PhraseCatalogFile.Read(
                PhraseCatalogFile.PathIn(Application.streamingAssetsPath), new CollectingProblemLog());

            Assert.IsNotEmpty(catalog.Get(key).Voice, key + " has an empty Voice, so it would be silent.");
        }

        [Test]
        public void EveryPhraseKeyHasATextAndEveryClipItSpeaksIsInTheVoiceFolder()
        {
            var problems = new CollectingProblemLog();
            PhraseCatalog catalog = PhraseCatalogFile.Read(PhraseCatalogFile.PathIn(Application.streamingAssetsPath), problems);
            List<string> clips = ClipNamesInVoiceFolder().ToList();
            Assert.IsNotEmpty(clips, "The voice folder holds no clips; the check would pass or fail for the wrong reason.");

            IReadOnlyList<string> missing = PhraseCatalogCheck.Missing(catalog, clips);

            Assert.IsEmpty(missing, string.Join("\n", missing));
        }
    }
}
