using System;
using System.IO;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class DiskWriteTests
    {
        [Test]
        public void TryRun_ASuccessfulWrite_ReturnsTrue_AndReportsNothing()
        {
            var problems = new CollectingProblemLog();
            bool ran = false;

            bool result = DiskWrite.TryRun(() => ran = true, problems, "save");

            Assert.IsTrue(result);
            Assert.IsTrue(ran);
            Assert.IsEmpty(problems.Messages);
        }

        [Test]
        public void TryRun_AnIoFailure_ReturnsFalse_AndReportsIt()
        {
            var problems = new CollectingProblemLog();

            bool result = DiskWrite.TryRun(() => throw new IOException("The disk is full."), problems, "save the answer");

            Assert.IsFalse(result);
            Assert.AreEqual(1, problems.Messages.Count);
            StringAssert.Contains("save the answer", problems.Messages[0]);
            StringAssert.Contains("The disk is full.", problems.Messages[0]);
        }

        [Test]
        public void TryRun_NoPermission_ReturnsFalse_AndReportsIt()
        {
            var problems = new CollectingProblemLog();

            bool result = DiskWrite.TryRun(() => throw new UnauthorizedAccessException("Access denied."), problems, "delete the profile");

            Assert.IsFalse(result);
            Assert.AreEqual(1, problems.Messages.Count);
        }

        [Test]
        public void TryRun_AnyOtherException_IsNotCaught()
        {
            var problems = new CollectingProblemLog();

            Assert.Throws<ArgumentException>(() => DiskWrite.TryRun(() => throw new ArgumentException("bug"), problems, "save"));
            Assert.IsEmpty(problems.Messages);
        }
    }
}
