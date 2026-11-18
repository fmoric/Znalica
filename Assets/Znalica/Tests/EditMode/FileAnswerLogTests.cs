using System.IO;
using System.Text;
using NUnit.Framework;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    public class FileAnswerLogTests
    {
        private TempFolder _temp;
        private CollectingProblemLog _problems;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempFolder();
            _problems = new CollectingProblemLog();
            _path = Path.Combine(_temp.Path, "Profile", "Answers.jsonl");
        }

        [TearDown]
        public void TearDown()
        {
            _temp.Dispose();
        }

        private FileAnswerLog Open()
        {
            return new FileAnswerLog(_path, _problems);
        }

        [Test]
        public void ReadAll_NoFileYet_IsEmpty()
        {
            Assert.IsEmpty(Open().ReadAll());
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void Append_SurvivesReopening_WithEveryFieldAsGiven()
        {
            Open().Append(TestRecords.Make(sessionNumber: 4, itemKey: "A:13-?=7", answer: "6", isCorrect: false, attempt: AttemptKind.Retry, roundId: "r-9"));

            AnswerRecord read = Open().ReadAll()[0];

            Assert.AreEqual(Activity.Arithmetic, read.Activity);
            Assert.AreEqual("A:13-?=7", read.ItemKey);
            Assert.AreEqual("6", read.Answer);
            Assert.IsFalse(read.IsCorrect);
            Assert.AreEqual(AttemptKind.Retry, read.Attempt);
            Assert.AreEqual(1234, read.ResponseMilliseconds);
            Assert.AreEqual("2026-11-17T10:30:00.0000000+01:00", read.AnsweredAt);
            Assert.AreEqual(4, read.SessionNumber);
            Assert.AreEqual("r-9", read.RoundId);
        }

        [Test]
        public void Append_SeveralRecordsAcrossReopenings_AreReadBackInOrderOnePerLine()
        {
            FileAnswerLog log = Open();
            log.Append(TestRecords.Make(itemKey: "A:1+1=?"));
            log.Append(TestRecords.Make(itemKey: "A:2+1=?"));
            Open().Append(TestRecords.Make(itemKey: "A:3+1=?"));

            var read = Open().ReadAll();

            Assert.AreEqual(3, read.Count);
            Assert.AreEqual("A:1+1=?", read[0].ItemKey);
            Assert.AreEqual("A:2+1=?", read[1].ItemKey);
            Assert.AreEqual("A:3+1=?", read[2].ItemKey);
            Assert.AreEqual(3, File.ReadAllLines(_path).Length);
        }

        [Test]
        public void ReadAll_LastLineCutAtAnyPoint_SkipsThatLineAndKeepsTheRest()
        {
            FileAnswerLog log = Open();
            log.Append(TestRecords.Make(itemKey: "A:1+1=?"));
            log.Append(TestRecords.Make(itemKey: "A:2+1=?"));
            string full = File.ReadAllText(_path);
            Assert.AreEqual(2, File.ReadAllLines(_path).Length, "Both records must be on disk before a line is cut.");
            int secondLineStart = full.IndexOf('\n') + 1;

            // Cut the second line after each possible number of characters, as a crash could (the cut at its very end,
            // before the line break, is a complete record and is not tried).
            for (int keep = 1; keep < full.Length - secondLineStart - 1; keep++)
            {
                File.WriteAllText(_path, full.Substring(0, secondLineStart + keep), new UTF8Encoding(false));

                var read = Open().ReadAll();

                Assert.AreEqual(1, read.Count, "A line cut after " + keep + " characters was read.");
                Assert.AreEqual("A:1+1=?", read[0].ItemKey);
            }
        }

        [Test]
        public void ReadAll_TruncatedLastLine_IsReported()
        {
            Open().Append(TestRecords.Make());
            File.AppendAllText(_path, "{\"Activity\":\"Arithm");

            Open().ReadAll();

            Assert.AreEqual(1, _problems.Messages.Count);
        }

        [Test]
        public void Append_AfterATruncatedLastLine_KeepsTheNewRecordReadable()
        {
            Open().Append(TestRecords.Make(itemKey: "A:1+1=?"));
            File.AppendAllText(_path, "{\"Activity\":\"Arithm");

            Open().Append(TestRecords.Make(itemKey: "A:2+1=?"));

            var read = Open().ReadAll();
            Assert.AreEqual(2, read.Count);
            Assert.AreEqual("A:2+1=?", read[1].ItemKey);
        }

        [Test]
        public void ReadAll_LineWithAnUnknownActivity_IsSkipped()
        {
            Open().Append(TestRecords.Make());
            File.AppendAllText(
                _path,
                "{\"Activity\":\"Origami\",\"ItemKey\":\"A:1+1=?\",\"Answer\":\"2\",\"IsCorrect\":true,\"Attempt\":\"First\"," +
                "\"ResponseMilliseconds\":1,\"AnsweredAt\":\"2026-11-17T10:30:00.0000000+01:00\",\"SessionNumber\":1,\"RoundId\":\"r\"}\n");

            Assert.AreEqual(1, Open().ReadAll().Count);
            Assert.AreEqual(1, _problems.Messages.Count);
        }

        [Test]
        public void Append_DoesNotRewriteWhatIsAlreadyOnDisk()
        {
            FileAnswerLog log = Open();
            log.Append(TestRecords.Make(itemKey: "A:1+1=?"));
            string before = File.ReadAllText(_path);

            log.Append(TestRecords.Make(itemKey: "A:2+1=?"));

            StringAssert.StartsWith(before, File.ReadAllText(_path));
        }
    }
}
