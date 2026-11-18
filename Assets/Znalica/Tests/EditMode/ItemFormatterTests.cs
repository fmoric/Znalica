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
        public void Tokens_OfAnItemThatIsNotArithmetic_Throws()
        {
            Assert.Throws<ArgumentException>(() => ItemFormatter.Tokens(new OtherItem()));
        }

        [TestCase(8, Operation.Addition, 5, "+5")]
        [TestCase(13, Operation.Subtraction, 6, "−6")]
        [TestCase(1, Operation.Addition, 12, "+12")]
        [TestCase(8, Operation.Addition, 0, "+0")]
        [TestCase(8, Operation.Subtraction, 0, "−0")]
        public void ChangeText_SignsTheJumpByItsDirection(int first, Operation operation, int second, string expected)
        {
            int result = operation == Operation.Addition ? first + second : first - second;
            Explanation explanation = new ArithmeticItem(first, operation, second, result, Unknown.Result).Explain();

            Assert.AreEqual(expected, ItemFormatter.ChangeText(explanation));
        }

        [Test]
        public void Text_OfAnArithmeticAndANumberLineItem_JoinsTheTokensWithSpaces()
        {
            Assert.AreEqual("8 + 5 = ?", ItemFormatter.Text(new ArithmeticItem(8, Operation.Addition, 5, 13, Unknown.Result)));
            Assert.AreEqual("6 +3 = ?", ItemFormatter.Text(NumberLineItem.Jump(6, 3)));
            Assert.AreEqual("? , 14", ItemFormatter.Text(NumberLineItem.Before(14)));
            Assert.AreEqual("14", ItemFormatter.Text(NumberLineItem.Place(14)));
        }

        [Test]
        public void Text_OfAComparisonItem_ShowsTheQuantitiesAndAMarkForTheSign()
        {
            Assert.AreEqual("7 ? 12", ItemFormatter.Text(ComparisonItem.ChooseSign(Representation.Numbers, 7, 12)));
            Assert.AreEqual("(12) ? (7)", ItemFormatter.Text(ComparisonItem.ChooseSign(Representation.Groups, 12, 7)));
        }

        [Test]
        public void Text_OfAnOrderingItem_ListsTheNumbersInTheOrderTheyAreShown()
        {
            Assert.AreEqual("14, 3, 9", ItemFormatter.Text(ComparisonItem.PutInOrder(new[] { 9, 3, 14 })));
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
