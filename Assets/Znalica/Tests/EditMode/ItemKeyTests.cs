using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ItemKeyTests
    {
        [TestCase("A:8+5=?")]
        [TestCase("A:13-?=7")]
        [TestCase("C:G7?12")]
        [TestCase("O:3,9,14")]
        [TestCase("L:Jump:6+3")]
        public void TryParse_ThenToString_GivesTheSameText(string text)
        {
            Assert.IsTrue(ItemKey.TryParse(text, out ItemKey key));

            Assert.AreEqual(text, key.ToString());
            Assert.IsTrue(ItemKey.TryParse(key.ToString(), out ItemKey again));
            Assert.AreEqual(key, again);
        }

        [Test]
        public void KeysOfTheSameText_AreEqual_AndOfDifferentText_AreNot()
        {
            ItemKey.TryParse("A:8+5=?", out ItemKey one);
            ItemKey.TryParse("A:8+5=?", out ItemKey same);
            ItemKey.TryParse("A:5+8=?", out ItemKey other);

            Assert.AreEqual(one, same);
            Assert.AreEqual(one.GetHashCode(), same.GetHashCode());
            Assert.AreNotEqual(one, other);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("A:8 + 5 = ?")]
        [TestCase("A:8−5=?")]
        [TestCase("A:8+5=?\n")]
        public void TryParse_TextThatIsNoKey_ReturnsFalse(string text)
        {
            Assert.IsFalse(ItemKey.TryParse(text, out _));
        }

        [Test]
        public void TryParse_TheLongestKeyAndTheEdgesOfPrintableAscii_AreAccepted()
        {
            Assert.IsTrue(ItemKey.TryParse(new string('A', 64), out _));
            Assert.IsTrue(ItemKey.TryParse("!", out _));
            Assert.IsTrue(ItemKey.TryParse("~", out _));
            Assert.IsFalse(ItemKey.TryParse(" ", out _));
            Assert.IsFalse(ItemKey.TryParse("\u007f", out _));
        }

        [Test]
        public void TryParse_TextLongerThanAnyKey_ReturnsFalse()
        {
            Assert.IsFalse(ItemKey.TryParse(new string('A', 65), out _));
        }
    }
}
