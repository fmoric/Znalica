using System.IO;
using NUnit.Framework;
using Znalica.Storage;

namespace Znalica.Tests.EditMode
{
    public class AtomicFileWriterTests
    {
        private TempFolder _temp;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempFolder();
        }

        [TearDown]
        public void TearDown()
        {
            _temp.Dispose();
        }

        [Test]
        public void Write_NewFileInNewFolder_CreatesItAndLeavesNoTemporaryFile()
        {
            string path = Path.Combine(_temp.Path, "Deep", "Settings.json");

            AtomicFileWriter.Write(path, "{\"a\":1}");

            Assert.AreEqual("{\"a\":1}", File.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + ".tmp"));
        }

        [Test]
        public void Write_ExistingFile_ReplacesItsContent()
        {
            string path = Path.Combine(_temp.Path, "Settings.json");
            AtomicFileWriter.Write(path, "old");

            AtomicFileWriter.Write(path, "new");

            Assert.AreEqual("new", File.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + ".tmp"));
        }

        [Test]
        public void Write_StaleTemporaryFileFromACrash_DoesNotTouchTheRealFileAndIsOverwrittenNextTime()
        {
            string path = Path.Combine(_temp.Path, "Settings.json");
            AtomicFileWriter.Write(path, "good");
            File.WriteAllText(path + ".tmp", "half-writt");

            Assert.AreEqual("good", File.ReadAllText(path));

            AtomicFileWriter.Write(path, "newer");

            Assert.AreEqual("newer", File.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + ".tmp"));
        }

        [Test]
        public void Write_Text_HasNoByteOrderMark()
        {
            string path = Path.Combine(_temp.Path, "Settings.json");

            AtomicFileWriter.Write(path, "{}");

            Assert.AreEqual(2, File.ReadAllBytes(path).Length);
        }
    }
}
