using System.IO;
using NUnit.Framework;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    public class SettingsStoreTests
    {
        private TempFolder _temp;
        private CollectingProblemLog _problems;
        private DataFolder _folder;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempFolder();
            _problems = new CollectingProblemLog();
            _folder = new DataFolder(_temp.Path);
        }

        [TearDown]
        public void TearDown()
        {
            _temp.Dispose();
        }

        private SettingsStore Store()
        {
            return new SettingsStore(_folder, _problems);
        }

        [Test]
        public void Load_NoFile_GivesFullVolumeWithoutAProblem()
        {
            Assert.AreEqual(1f, Store().Load().VoiceVolume);
            Assert.IsEmpty(_problems.Messages);
        }

        [Test]
        public void Save_ThenLoadInAnotherStore_GivesTheSavedVolume()
        {
            Store().Save(new Settings { VoiceVolume = 0.4f });

            Assert.AreEqual(0.4f, Store().Load().VoiceVolume, 0.0001f);
        }

        [TestCase(1.7f, 1f)]
        [TestCase(-0.3f, 0f)]
        public void Save_VolumeOutsideTheRange_IsKeptInsideIt(float given, float expected)
        {
            Store().Save(new Settings { VoiceVolume = given });

            Assert.AreEqual(expected, Store().Load().VoiceVolume);
        }

        [Test]
        public void Load_FileWithAVolumeOutsideTheRange_IsClamped()
        {
            File.WriteAllText(_folder.SettingsPath, "{\"VoiceVolume\":5}");

            Assert.AreEqual(1f, Store().Load().VoiceVolume);
        }

        [TestCase("not json at all")]
        [TestCase("")]
        public void Load_UnreadableFile_GivesDefaultsAndReportsIt(string content)
        {
            File.WriteAllText(_folder.SettingsPath, content);

            Assert.AreEqual(1f, Store().Load().VoiceVolume);
            Assert.AreEqual(1, _problems.Messages.Count);
        }
    }
}
