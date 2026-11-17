using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class VoiceVolumeTests
    {
        [TestCase(0f)]
        [TestCase(-0.5f)]
        public void IsSilent_VolumeZeroOrBelow_IsTrue(float volume)
        {
            Assert.IsTrue(VoiceVolume.IsSilent(volume));
        }

        [TestCase(0.01f)]
        [TestCase(1f)]
        [TestCase(3f)]
        public void IsSilent_AnyAudibleVolume_IsFalse(float volume)
        {
            Assert.IsFalse(VoiceVolume.IsSilent(volume));
        }

        [Test]
        public void Clamp_KeepsTheVolumeBetweenZeroAndOne()
        {
            Assert.AreEqual(0f, VoiceVolume.Clamp(-1f));
            Assert.AreEqual(0.4f, VoiceVolume.Clamp(0.4f));
            Assert.AreEqual(1f, VoiceVolume.Clamp(2f));
        }
    }
}
