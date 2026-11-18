using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ProfileNameTests
    {
        [TestCase("Mira", "Mira")]
        [TestCase("  Mira  ", "Mira")]
        [TestCase("Ana-Marija", "Ana-Marija")]
        [TestCase("Luka Ivo", "Luka Ivo")]
        [TestCase("Žana Đurđa", "Žana Đurđa")]
        [TestCase("A", "A")]
        public void TryNormalize_AcceptableName_GivesTheTrimmedName(string input, string expected)
        {
            Assert.IsTrue(ProfileName.TryNormalize(input, out string name));
            Assert.AreEqual(expected, name);
        }

        [Test]
        public void TryNormalize_TwentyLetters_IsAccepted_TwentyOneIsNot()
        {
            Assert.IsTrue(ProfileName.TryNormalize(new string('a', 20), out _));
            Assert.IsFalse(ProfileName.TryNormalize(new string('a', 21), out _));
        }

        [Test]
        public void TryNormalize_LongOnlyBeforeTrimming_IsAccepted()
        {
            Assert.IsTrue(ProfileName.TryNormalize("   " + new string('a', 20) + "   ", out _));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Mira 7")]
        [TestCase("Mira.")]
        [TestCase("Mira/Luka")]
        [TestCase("Mira\nLuka")]
        [TestCase("2010-05-01")]
        [TestCase("12345678901234567890")]
        public void TryNormalize_EmptyOrWithOtherCharacters_IsRefused(string input)
        {
            Assert.IsFalse(ProfileName.TryNormalize(input, out string name));
            Assert.AreEqual(string.Empty, name);
        }
    }
}
