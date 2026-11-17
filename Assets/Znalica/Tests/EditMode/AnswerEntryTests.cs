using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class AnswerEntryTests
    {
        private static AnswerEntry Typed(params int[] digits)
        {
            var entry = new AnswerEntry();
            foreach (int digit in digits)
            {
                Assert.IsTrue(entry.TryType(digit), "digit " + digit + " was refused");
            }

            return entry;
        }

        [Test]
        public void NewEntry_IsEmpty()
        {
            Assert.IsNull(new AnswerEntry().Value);
        }

        [Test]
        public void TryType_AfterTwo_OnlyZeroIsTaken()
        {
            AnswerEntry entry = Typed(2);

            for (int digit = 1; digit <= 9; digit++)
            {
                Assert.IsFalse(entry.TryType(digit), "digit " + digit + " would make 2" + digit);
                Assert.AreEqual(2, entry.Value);
            }

            Assert.IsTrue(entry.TryType(0));
            Assert.AreEqual(20, entry.Value);
        }

        [Test]
        public void TryType_AfterOne_EveryDigitIsTaken()
        {
            for (int digit = 0; digit <= 9; digit++)
            {
                Assert.AreEqual(10 + digit, Typed(1, digit).Value);
            }
        }

        [Test]
        public void TryType_AfterTwenty_EveryDigitIsRefused()
        {
            AnswerEntry entry = Typed(2, 0);

            for (int digit = 0; digit <= 9; digit++)
            {
                Assert.IsFalse(entry.TryType(digit));
            }

            Assert.AreEqual(20, entry.Value);
        }

        [TestCase(3)]
        [TestCase(9)]
        public void TryType_AfterAnyDigitAboveTwo_AnotherDigitIsRefused(int first)
        {
            AnswerEntry entry = Typed(first);

            Assert.IsFalse(entry.TryType(0));
            Assert.AreEqual(first, entry.Value);
        }

        [Test]
        public void TryType_NotADigit_IsRefused()
        {
            var entry = new AnswerEntry();

            Assert.IsFalse(entry.TryType(-1));
            Assert.IsFalse(entry.TryType(10));
            Assert.IsNull(entry.Value);
        }

        [Test]
        public void TryType_ZeroThenADigit_GivesTheDigit()
        {
            AnswerEntry entry = Typed(0);
            Assert.AreEqual(0, entry.Value);

            Assert.IsTrue(entry.TryType(7));

            Assert.AreEqual(7, entry.Value);
        }

        [Test]
        public void Erase_RemovesTheLastDigit_AndAnEmptyBoxStaysEmpty()
        {
            AnswerEntry entry = Typed(1, 7);

            entry.Erase();
            Assert.AreEqual(1, entry.Value);
            entry.Erase();
            Assert.IsNull(entry.Value);
            entry.Erase();
            Assert.IsNull(entry.Value);
        }

        [Test]
        public void Erase_AfterTwenty_LeavesTwo_AndAnyDigitThenFollowsTheRuleAgain()
        {
            AnswerEntry entry = Typed(2, 0);

            entry.Erase();

            Assert.AreEqual(2, entry.Value);
            Assert.IsFalse(entry.TryType(5));
            Assert.IsTrue(entry.TryType(0));
        }

        [Test]
        public void NoSequenceOfKeystrokesCanLeaveTheBoxAboveTwenty()
        {
            // Every sequence of up to four digits, whatever is refused along the way.
            for (int length = 1; length <= 4; length++)
            {
                int count = (int)System.Math.Pow(10, length);
                for (int n = 0; n < count; n++)
                {
                    var entry = new AnswerEntry();
                    int rest = n;
                    for (int i = 0; i < length; i++)
                    {
                        entry.TryType(rest % 10);
                        rest /= 10;
                        Assert.IsTrue(!entry.Value.HasValue || NumberRange.Contains(entry.Value.Value), "sequence " + n);
                    }
                }
            }
        }
    }
}
