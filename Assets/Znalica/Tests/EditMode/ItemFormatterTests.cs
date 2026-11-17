using System;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ItemFormatterTests
    {
        private static string[] Texts(Item item)
        {
            return ItemFormatter.Tokens(item).Select(t => t.Text).ToArray();
        }

        [Test]
        public void Addition_ResultUnknown_ShowsTheSumWithTheMarkLast()
        {
            var item = new ArithmeticItem(8, Operation.Addition, 5, 13, Unknown.Result);

            Assert.AreEqual(new[] { "8", "+", "5", "=", "?" }, Texts(item));
            Assert.AreEqual(new[] { false, false, false, false, true }, ItemFormatter.Tokens(item).Select(t => t.IsUnknown).ToArray());
        }

        [Test]
        public void Addition_SecondUnknown_MarksTheSecondNumber()
        {
            var item = new ArithmeticItem(8, Operation.Addition, 5, 13, Unknown.Second);

            Assert.AreEqual(new[] { "8", "+", "?", "=", "13" }, Texts(item));
            Assert.AreEqual(new[] { false, false, true, false, false }, ItemFormatter.Tokens(item).Select(t => t.IsUnknown).ToArray());
        }

        [Test]
        public void Addition_FirstUnknown_MarksTheFirstNumber()
        {
            var item = new ArithmeticItem(8, Operation.Addition, 5, 13, Unknown.First);

            Assert.AreEqual(new[] { "?", "+", "5", "=", "13" }, Texts(item));
            Assert.AreEqual(new[] { true, false, false, false, false }, ItemFormatter.Tokens(item).Select(t => t.IsUnknown).ToArray());
        }

        [Test]
        public void Subtraction_ShowsTheMinusSignNotTheKeysHyphen()
        {
            var item = new ArithmeticItem(13, Operation.Subtraction, 6, 7, Unknown.Result);

            string[] texts = Texts(item);

            Assert.AreEqual("−", texts[1]);
            Assert.AreNotEqual("-", texts[1]);
            Assert.AreEqual(new[] { "13", "−", "6", "=", "?" }, texts);
        }

        [Test]
        public void Number_WithTwoDigitsAndZero_IsWrittenInFull()
        {
            var item = new ArithmeticItem(0, Operation.Addition, 20, 20, Unknown.Result);

            Assert.AreEqual(new[] { "0", "+", "20", "=", "?" }, Texts(item));
        }

        [Test]
        public void Tokens_OfAnItemThatIsNotArithmetic_Throws()
        {
            Assert.Throws<ArgumentException>(() => ItemFormatter.Tokens(new OtherItem()));
        }

        [TestCase(5, "+5")]
        [TestCase(-6, "−6")]
        [TestCase(12, "+12")]
        public void ChangeText_SignsTheJump(int change, string expected)
        {
            Assert.AreEqual(expected, ItemFormatter.ChangeText(change));
        }

        private sealed class OtherItem : Item
        {
            public override ItemKey Key
            {
                get
                {
                    ItemKey.TryParse("X:1", out ItemKey key);
                    return key;
                }
            }

            public override Activity Activity => Activity.Comparison;

            public override Difficulty Difficulty => Difficulty.WithinTen;

            public override Explanation Explain()
            {
                return null;
            }

            public override bool IsCorrect(Answer answer)
            {
                return false;
            }
        }
    }
}
