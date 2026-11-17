using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class PhraseSlotTests
    {
        [TestCase("{Start}", "Start")]
        [TestCase("{a_1}", "a_1")]
        public void TryParse_SlotName_IsAccepted(string item, string name)
        {
            Assert.IsTrue(PhraseSlot.TryParse(item, out string parsed));
            Assert.AreEqual(name, parsed);
        }

        [TestCase("{a b}")]
        [TestCase("{!}")]
        [TestCase("{}")]
        [TestCase("{Start}\n")]
        [TestCase("ThenMore")]
        [TestCase(null)]
        public void TryParse_NotASlotName_IsRefused(string item)
        {
            Assert.IsFalse(PhraseSlot.TryParse(item, out string parsed));
            Assert.IsNull(parsed);
        }

        [TestCase("{Start}")]
        [TestCase("{a b}")]
        [TestCase("{!}")]
        [TestCase("{a_1}")]
        public void SlotInVoiceAndSlotInText_AgreeOnWhatIsAName(string item)
        {
            Assert.AreEqual(PhraseSlot.InText.IsMatch(item), PhraseSlot.TryParse(item, out _));
        }
    }
}
