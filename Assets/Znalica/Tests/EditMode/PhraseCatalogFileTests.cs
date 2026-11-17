using System.IO;
using System.Text;
using NUnit.Framework;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    public class PhraseCatalogFileTests
    {
        private const string SmallFile = @"{ ""Entries"": [
  { ""Key"": ""PraiseRight"", ""Text"": ""Bravo!"", ""Voice"": [""PraiseRight""] },
  { ""Key"": ""ExplainJump"", ""Text"": ""{Start}, pa još {Change}, to je {End}."",
    ""Voice"": [""{Start}"", ""ThenMore"", ""{Change}"", ""Is"", ""{End}""] },
  { ""Key"": ""ButtonNext"", ""Text"": ""Dalje"", ""Voice"": [] }
] }";

        private CollectingProblemLog _problems;

        [SetUp]
        public void SetUp()
        {
            _problems = new CollectingProblemLog();
        }

        [Test]
        public void Parse_SmallWrappedFile_GivesTextAndVoiceOfEachEntry()
        {
            PhraseCatalog catalog = PhraseCatalogFile.Parse(SmallFile, _problems);

            Assert.AreEqual("Bravo!", catalog.Get(PhraseKey.PraiseRight).Text);
            CollectionAssert.AreEqual(new[] { "PraiseRight" }, catalog.Get(PhraseKey.PraiseRight).Voice);
            Assert.AreEqual("{Start}, pa još {Change}, to je {End}.", catalog.Get(PhraseKey.ExplainJump).Text);
            CollectionAssert.AreEqual(
                new[] { "{Start}", "ThenMore", "{Change}", "Is", "{End}" }, catalog.Get(PhraseKey.ExplainJump).Voice);
            Assert.AreEqual("Dalje", catalog.Get(PhraseKey.ButtonNext).Text);
            Assert.IsEmpty(catalog.Get(PhraseKey.ButtonNext).Voice);
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void Parse_KeyNotInTheFile_IsNotInTheCatalogAndIsFlaggedByTheCheck()
        {
            PhraseCatalog catalog = PhraseCatalogFile.Parse(SmallFile, _problems);

            Assert.IsFalse(catalog.Contains(PhraseKey.ActivityNameArithmetic));
            Assert.IsTrue(PhraseCatalogCheck.Missing(catalog, new string[0]).Count > 0);
            CollectionAssert.Contains(PhraseCatalogCheck.Missing(catalog, new string[0]), "No entry for ActivityNameArithmetic.");
        }

        [Test]
        public void Parse_NotJson_ReportsAndGivesAnEmptyCatalog()
        {
            PhraseCatalog catalog = PhraseCatalogFile.Parse("this is not json", _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("not valid JSON", _problems.Messages[0]);
            Assert.IsEmpty(catalog.Entries);
        }

        [TestCase("")]
        [TestCase("   \r\n\t ")]
        public void Parse_EmptyOrWhitespaceOnly_ReportsOnceAndGivesAnEmptyCatalog(string json)
        {
            PhraseCatalog catalog = PhraseCatalogFile.Parse(json, _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            Assert.IsEmpty(catalog.Entries);
        }

        [Test]
        public void Parse_TopLevelArray_ReportsBecauseTheListMustBeWrapped()
        {
            PhraseCatalog catalog = PhraseCatalogFile.Parse(@"[ { ""Key"": ""PraiseRight"", ""Text"": ""Bravo!"" } ]", _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            Assert.IsEmpty(catalog.Entries);
        }

        [Test]
        public void Parse_ObjectWithoutEntries_Reports()
        {
            PhraseCatalog catalog = PhraseCatalogFile.Parse("{ }", _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("Entries", _problems.Messages[0]);
            Assert.IsEmpty(catalog.Entries);
        }

        [Test]
        public void Parse_UnknownKey_IsReportedAndTheOtherEntriesAreKept()
        {
            const string json = @"{ ""Entries"": [
  { ""Key"": ""NoSuchPhrase"", ""Text"": ""x"", ""Voice"": [] },
  { ""Key"": ""PraiseRight"", ""Text"": ""Bravo!"", ""Voice"": [""PraiseRight""] }
] }";

            PhraseCatalog catalog = PhraseCatalogFile.Parse(json, _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("NoSuchPhrase", _problems.Messages[0]);
            Assert.AreEqual("Bravo!", catalog.Get(PhraseKey.PraiseRight).Text);
        }

        [TestCase("0")]
        [TestCase("99")]
        [TestCase("praiseright")]
        [TestCase(" PraiseRight")]
        public void Parse_KeyThatIsNotAnExactMemberName_IsAnUnknownKey(string key)
        {
            string json = @"{ ""Entries"": [ { ""Key"": """ + key + @""", ""Text"": ""x"", ""Voice"": [] } ] }";

            PhraseCatalog catalog = PhraseCatalogFile.Parse(json, _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            Assert.IsEmpty(catalog.Entries);
        }

        [Test]
        public void Parse_KeyTwice_IsReportedAndTheLastOneIsUsed()
        {
            const string json = @"{ ""Entries"": [
  { ""Key"": ""PraiseRight"", ""Text"": ""Prvi"", ""Voice"": [] },
  { ""Key"": ""PraiseRight"", ""Text"": ""Drugi"", ""Voice"": [] }
] }";

            PhraseCatalog catalog = PhraseCatalogFile.Parse(json, _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("PraiseRight", _problems.Messages[0]);
            Assert.AreEqual("Drugi", catalog.Get(PhraseKey.PraiseRight).Text);
        }

        [Test]
        public void Read_Utf8FileWithCroatianLetters_KeepsTheLetters()
        {
            string path = Path.Combine(Path.GetTempPath(), "Znalica-PhraseCatalogFileTests-" + Path.GetRandomFileName() + ".json");
            const string json = @"{ ""Entries"": [ { ""Key"": ""ButtonNext"", ""Text"": ""Čćđšž ČĆĐŠŽ − Računanje"", ""Voice"": [] } ] }";
            File.WriteAllText(path, json, new UTF8Encoding(true));
            try
            {
                PhraseCatalog catalog = PhraseCatalogFile.Read(path, _problems);

                Assert.AreEqual("Čćđšž ČĆĐŠŽ − Računanje", catalog.Get(PhraseKey.ButtonNext).Text);
                Assert.IsEmpty(_problems.Messages);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void Read_FileThatDoesNotExist_ReportsAndGivesAnEmptyCatalog()
        {
            string path = Path.Combine(Path.GetTempPath(), "Znalica-NoSuchFile-" + Path.GetRandomFileName() + ".json");

            PhraseCatalog catalog = PhraseCatalogFile.Read(path, _problems);

            Assert.AreEqual(1, _problems.Messages.Count);
            StringAssert.Contains("not found", _problems.Messages[0]);
            Assert.IsEmpty(catalog.Entries);
        }

        [Test]
        public void PathIn_PutsTheFileInTheTextFolderOfStreamingAssets()
        {
            string path = PhraseCatalogFile.PathIn("Root").Replace('\\', '/');

            Assert.AreEqual("Root/Text/Hr.json", path);
        }
    }
}
