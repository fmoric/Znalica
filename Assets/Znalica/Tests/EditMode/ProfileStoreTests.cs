using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    /// <summary>The profile store over a temporary data folder. Names are invented; no real child's name is used.</summary>
    public class ProfileStoreTests
    {
        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 11, 17, 10, 30, 0, TimeSpan.FromHours(1));

        private TempFolder _temp;
        private CollectingProblemLog _problems;
        private DataFolder _folder;
        private FixedClock _clock;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempFolder();
            _problems = new CollectingProblemLog();
            _folder = new DataFolder(_temp.Path);
            _clock = new FixedClock(Now);
        }

        [TearDown]
        public void TearDown()
        {
            _temp.Dispose();
        }

        private ProfileStore Store()
        {
            return new ProfileStore(_folder, _clock, _problems);
        }

        [Test]
        public void List_NothingCreated_IsEmpty()
        {
            Assert.IsEmpty(Store().List());
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void Create_ThenListInAnotherStore_GivesTheProfileWithItsFields()
        {
            Profile created = Store().Create("  Mira Test ", "Fox");

            Profile read = Store().List().Single();

            Assert.AreEqual(created.ProfileId, read.ProfileId);
            Assert.AreEqual("Mira Test", read.DisplayName);
            Assert.AreEqual("Fox", read.PictureId);
            Assert.AreEqual(Now.ToString("o"), read.CreatedAt);
            Assert.IsTrue(Directory.Exists(_folder.ProfileFolder(created.ProfileId)));
        }

        [Test]
        public void List_SeveralProfiles_AreOldestFirst()
        {
            Store().Create("Prva", "Fox");
            _clock.Now = Now.AddMinutes(5);
            Store().Create("Druga", "Owl");
            _clock.Now = Now.AddMinutes(-5);
            Store().Create("Nulta", "Cat");

            Assert.AreEqual(new[] { "Nulta", "Prva", "Druga" }, Store().List().Select(p => p.DisplayName).ToArray());
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Mira7")]
        [TestCase("Mira Test Testic Testovic X")]
        [TestCase("Mira/..")]
        public void Create_InvalidName_IsRefusedAndNothingIsWritten(string name)
        {
            Assert.Throws<ArgumentException>(() => Store().Create(name, "Fox"));

            Assert.IsEmpty(Store().List());
            Assert.IsFalse(Directory.Exists(_folder.ProfilesPath) && Directory.GetDirectories(_folder.ProfilesPath).Length > 0);
        }

        [Test]
        public void Create_UnknownPicture_IsRefused()
        {
            Assert.Throws<ArgumentException>(() => Store().Create("Mira", "Dragon"));
        }

        [Test]
        public void Rename_ChangesOnlyTheName()
        {
            Profile created = Store().Create("Mira", "Owl");

            Store().Rename(created.ProfileId, "Luka-Ivo");

            Profile read = Store().List().Single();
            Assert.AreEqual("Luka-Ivo", read.DisplayName);
            Assert.AreEqual("Owl", read.PictureId);
            Assert.AreEqual(created.ProfileId, read.ProfileId);
            Assert.AreEqual(created.CreatedAt, read.CreatedAt);
        }

        [Test]
        public void Rename_InvalidName_KeepsTheOldOne()
        {
            Profile created = Store().Create("Mira", "Owl");

            Assert.Throws<ArgumentException>(() => Store().Rename(created.ProfileId, "x1"));

            Assert.AreEqual("Mira", Store().List().Single().DisplayName);
        }

        [Test]
        public void Rename_UnknownProfile_IsRefused()
        {
            Assert.Throws<ArgumentException>(() => Store().Rename(Guid.NewGuid().ToString(), "Mira"));
        }

        [Test]
        public void Delete_RemovesTheProfilesFolderWithItsAnswers()
        {
            ProfileStore store = Store();
            Profile mira = store.Create("Mira", "Fox");
            store.AnswerLog(mira.ProfileId).Append(TestRecords.Make());
            Assert.IsTrue(File.Exists(_folder.AnswersPath(mira.ProfileId)));

            bool deleted = store.Delete(mira.ProfileId);

            Assert.IsTrue(deleted);
            Assert.IsFalse(Directory.Exists(_folder.ProfileFolder(mira.ProfileId)));
            Assert.IsEmpty(Store().List());
        }

        [Test]
        public void Delete_OneProfile_LeavesTheOthersAndTheirAnswers()
        {
            ProfileStore store = Store();
            Profile mira = store.Create("Mira", "Fox");
            Profile luka = store.Create("Luka", "Bear");
            store.AnswerLog(luka.ProfileId).Append(TestRecords.Make(itemKey: "A:1+1=?"));

            store.Delete(mira.ProfileId);

            Assert.AreEqual(luka.ProfileId, store.List().Single().ProfileId);
            Assert.AreEqual("A:1+1=?", store.AnswerLog(luka.ProfileId).ReadAll().Single().ItemKey);
        }

        [Test]
        public void Delete_UnknownProfile_ReturnsFalse()
        {
            Assert.IsFalse(Store().Delete(Guid.NewGuid().ToString()));
        }

        [TestCase("..")]
        [TestCase("..\\..")]
        [TestCase("not-a-guid")]
        [TestCase("")]
        public void Delete_IdThatIsNotAGuid_IsRefusedAndDeletesNothing(string id)
        {
            string outside = Path.Combine(_temp.Path, "Outside.txt");
            File.WriteAllText(outside, "keep");
            Store().Create("Mira", "Fox");

            Assert.Throws<ArgumentException>(() => Store().Delete(id));

            Assert.IsTrue(File.Exists(outside));
            Assert.AreEqual(1, Store().List().Count);
        }

        [Test]
        public void AnswerLogs_OfTwoProfiles_NeverShareRecords()
        {
            ProfileStore store = Store();
            Profile mira = store.Create("Mira", "Fox");
            Profile luka = store.Create("Luka", "Bear");

            store.AnswerLog(mira.ProfileId).Append(TestRecords.Make(itemKey: "A:1+1=?", sessionNumber: 3));
            store.AnswerLog(luka.ProfileId).Append(TestRecords.Make(itemKey: "A:2+2=?", sessionNumber: 1));
            store.AnswerLog(luka.ProfileId).Append(TestRecords.Make(itemKey: "A:3+3=?", sessionNumber: 1));

            Assert.AreEqual(new[] { "A:1+1=?" }, store.AnswerLog(mira.ProfileId).ReadAll().Select(r => r.ItemKey).ToArray());
            Assert.AreEqual(new[] { "A:2+2=?", "A:3+3=?" }, store.AnswerLog(luka.ProfileId).ReadAll().Select(r => r.ItemKey).ToArray());
            Assert.AreEqual(4, SessionNumber.Next(store.AnswerLog(mira.ProfileId).ReadAll()));
            Assert.AreEqual(2, SessionNumber.Next(store.AnswerLog(luka.ProfileId).ReadAll()));
        }

        [Test]
        public void List_FolderWithoutAReadableProfileFile_IsSkippedAndReported()
        {
            Store().Create("Mira", "Fox");
            string stray = _folder.ProfileFolder(Guid.NewGuid().ToString());
            Directory.CreateDirectory(stray);
            File.WriteAllText(Path.Combine(stray, "Profile.json"), "not json");

            Assert.AreEqual(1, Store().List().Count);
            Assert.AreEqual(1, _problems.Messages.Count);
        }

        [Test]
        public void List_ProfileFileThatNamesAnotherId_IsNotThatProfile()
        {
            Profile mira = Store().Create("Mira", "Fox");
            string copy = _folder.ProfileFolder(Guid.NewGuid().ToString());
            Directory.CreateDirectory(copy);
            File.Copy(_folder.ProfilePath(mira.ProfileId), Path.Combine(copy, "Profile.json"));

            Assert.AreEqual(1, Store().List().Count);
        }
    }
}
