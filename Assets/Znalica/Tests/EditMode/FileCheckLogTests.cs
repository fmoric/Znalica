using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    /// <summary>The check log over a temporary data folder. Names are invented; no real child's name is used.</summary>
    public class FileCheckLogTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 11, 18, 15, 0, 0, TimeSpan.FromHours(1));

        private TempFolder _temp;
        private CollectingProblemLog _problems;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempFolder();
            _problems = new CollectingProblemLog();
            _path = Path.Combine(_temp.Path, "Profile", "Checks.jsonl");
        }

        [TearDown]
        public void TearDown()
        {
            _temp.Dispose();
        }

        private FileCheckLog Open()
        {
            return new FileCheckLog(_path, _problems);
        }

        private static CheckLine Answer(string id, string key, string answer = "13", bool correct = true)
        {
            return CheckLine.ForAnswer(id, key, answer, correct, "2026-11-18T15:00:00.0000000+01:00");
        }

        [Test]
        public void ReadAll_NoFileYet_IsEmpty()
        {
            Assert.IsEmpty(Open().ReadAll());
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void Append_AnAnswerAndAClosingLine_SurviveReopening_WithEveryField()
        {
            Open().Append(Answer("c-1", "L:Before:14", "9", false));
            Open().Append(CheckLine.ForClosing("c-1", "2026-11-18T15:09:00.0000000+01:00", 17, 20));

            CheckLine[] read = Open().ReadAll().ToArray();

            Assert.AreEqual(2, read.Length);
            Assert.IsFalse(read[0].IsClosing);
            Assert.AreEqual("c-1", read[0].CheckId);
            Assert.AreEqual("L:Before:14", read[0].ItemKey);
            Assert.AreEqual("9", read[0].Answer);
            Assert.IsFalse(read[0].IsCorrect);
            Assert.AreEqual("2026-11-18T15:00:00.0000000+01:00", read[0].AnsweredAt);
            Assert.IsTrue(read[1].IsClosing);
            Assert.AreEqual("c-1", read[1].CheckId);
            Assert.AreEqual("2026-11-18T15:09:00.0000000+01:00", read[1].CompletedAt);
            Assert.AreEqual(17, read[1].Score);
            Assert.AreEqual(20, read[1].ItemCount);
            Assert.AreEqual(2, File.ReadAllLines(_path).Length);
        }

        [Test]
        public void Append_AnItemLeftEmpty_ReadsBackAsAnEmptyAnswer()
        {
            Open().Append(Answer("c-1", "L:Before:14", string.Empty, false));

            CheckLine read = Open().ReadAll().Single();

            Assert.AreEqual(string.Empty, read.Answer);
            Assert.IsFalse(read.IsCorrect);
        }

        [Test]
        public void ReadAll_LastLineCutAtAnyPoint_SkipsThatLineAndKeepsTheRest()
        {
            FileCheckLog log = Open();
            log.Append(Answer("c-1", "L:Before:14"));
            log.Append(CheckLine.ForClosing("c-1", "2026-11-18T15:09:00.0000000+01:00", 1, 1));
            string full = File.ReadAllText(_path);
            int secondLineStart = full.IndexOf('\n') + 1;

            for (int keep = 1; keep < full.Length - secondLineStart - 1; keep++)
            {
                File.WriteAllText(_path, full.Substring(0, secondLineStart + keep), new UTF8Encoding(false));

                CheckLine[] read = Open().ReadAll().ToArray();

                Assert.AreEqual(1, read.Length, "A line cut after " + keep + " characters was read.");
                Assert.IsFalse(read[0].IsClosing);
            }
        }

        [Test]
        public void ACutClosingLine_LeavesTheCheckUnfinishedWithNoScore()
        {
            Open().Append(Answer("c-1", "L:Before:14"));
            File.AppendAllText(_path, "{\"Line\":\"Closing\",\"CheckId\":\"c-1\",\"CompletedAt\":\"2026-11-18T15:09");

            CheckResult result = CheckHistory.From(Open().ReadAll()).Single();

            Assert.IsFalse(result.IsFinished);
            Assert.IsNull(result.Score);
            Assert.AreEqual(1, _problems.Messages.Count);
        }

        [Test]
        public void Append_AfterACutLastLine_KeepsTheNewLineReadable()
        {
            Open().Append(Answer("c-1", "L:Before:14"));
            File.AppendAllText(_path, "{\"Line\":\"Answer\",\"CheckId\":\"c-1\",\"Item");

            Open().Append(Answer("c-1", "L:After:12"));

            CheckLine[] read = Open().ReadAll().ToArray();
            Assert.AreEqual(2, read.Length);
            Assert.AreEqual("L:After:12", read[1].ItemKey);
        }

        [Test]
        public void ReadAll_ALineOfAnUnknownKindOrWithAScoreAboveTheItemCount_IsSkipped()
        {
            Open().Append(Answer("c-1", "L:Before:14"));
            File.AppendAllText(_path, "{\"Line\":\"Mystery\",\"CheckId\":\"c-1\",\"ItemKey\":\"L:Before:3\",\"AnsweredAt\":\"2026-11-18T15:00:00.0000000+01:00\"}\n");
            File.AppendAllText(_path, "{\"Line\":\"Closing\",\"CheckId\":\"c-1\",\"CompletedAt\":\"2026-11-18T15:09:00.0000000+01:00\",\"Score\":21,\"ItemCount\":20}\n");

            Assert.AreEqual(1, Open().ReadAll().Count);
            Assert.AreEqual(2, _problems.Messages.Count);
        }

        [Test]
        public void ChecksOfSeveralDays_AreListedInTheOrderTheyWereTaken_EachWithItsOwnScore()
        {
            FileCheckLog log = Open();
            log.Append(CheckLine.ForAnswer("first", "L:Before:14", "13", true, "2026-11-10T10:00:00.0000000+01:00"));
            log.Append(CheckLine.ForClosing("first", "2026-11-10T10:05:00.0000000+01:00", 1, 1));
            log.Append(CheckLine.ForAnswer("second", "L:Before:7", "6", true, "2026-11-14T10:00:00.0000000+01:00"));
            log.Append(CheckLine.ForAnswer("third", "L:Before:9", "8", true, "2026-11-17T10:00:00.0000000+01:00"));
            log.Append(CheckLine.ForClosing("third", "2026-11-17T10:05:00.0000000+01:00", 1, 1));

            var results = CheckHistory.From(Open().ReadAll());

            CollectionAssert.AreEqual(new[] { "first", "second", "third" }, results.Select(result => result.CheckId).ToArray());
            CollectionAssert.AreEqual(new bool[] { true, false, true }, results.Select(result => result.IsFinished).ToArray());
            Assert.IsNull(results[1].Score);
        }

        [Test]
        public void ACheckTakenThroughTheProfileStore_ReachesChecksJsonl_AndNeverAnswersJsonl()
        {
            var folder = new DataFolder(_temp.Path);
            var store = new ProfileStore(folder, new FixedClock(Now), _problems);
            Profile profile = store.Create("Mira Test", "Fox");
            var session = new SchoolCheckSession(
                SchoolCheckPlanner.Plan(new SeededRandom(5)), store.CheckLog(profile.ProfileId), new FixedClock(Now), "c-1");
            CheckAnswers.FillPageRight(session);
            session.FinishPage();
            CheckAnswers.FillPageRight(session);
            session.FinishPage();

            Assert.IsTrue(File.Exists(folder.ChecksPath(profile.ProfileId)));
            Assert.IsFalse(File.Exists(folder.AnswersPath(profile.ProfileId)), "A check wrote an answers file.");
            Assert.IsEmpty(store.AnswerLog(profile.ProfileId).ReadAll());
            Assert.IsEmpty(ItemSchedule.Build(store.AnswerLog(profile.ProfileId).ReadAll()).Entries);
            Assert.AreEqual(21, store.CheckLog(profile.ProfileId).ReadAll().Count);
            Assert.AreEqual(20, CheckHistory.From(store.CheckLog(profile.ProfileId).ReadAll()).Single().Score);
        }

        [Test]
        public void ACheckAnsweredAfterTheGame_LeavesTheGamesAnswersAsTheyWere()
        {
            var folder = new DataFolder(_temp.Path);
            var store = new ProfileStore(folder, new FixedClock(Now), _problems);
            Profile profile = store.Create("Mira Test", "Fox");
            store.AnswerLog(profile.ProfileId).Append(TestRecords.Make(itemKey: "A:1+1=?", answer: "2"));
            string before = File.ReadAllText(folder.AnswersPath(profile.ProfileId));

            var session = new SchoolCheckSession(
                SchoolCheckPlanner.Plan(new SeededRandom(6)), store.CheckLog(profile.ProfileId), new FixedClock(Now), "c-2");
            CheckAnswers.FillPageRight(session);
            session.FinishPage();

            Assert.AreEqual(before, File.ReadAllText(folder.AnswersPath(profile.ProfileId)));
        }

        [Test]
        public void DeletingAProfile_TakesItsChecksWithIt()
        {
            var folder = new DataFolder(_temp.Path);
            var store = new ProfileStore(folder, new FixedClock(Now), _problems);
            Profile profile = store.Create("Mira Test", "Fox");
            store.CheckLog(profile.ProfileId).Append(Answer("c-1", "L:Before:14"));

            Assert.IsTrue(store.Delete(profile.ProfileId));

            Assert.IsFalse(File.Exists(folder.ChecksPath(profile.ProfileId)));
        }
    }
}
