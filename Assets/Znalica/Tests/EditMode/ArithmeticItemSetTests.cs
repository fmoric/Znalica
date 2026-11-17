using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ArithmeticItemSetTests
    {
        private static readonly ArithmeticItemSet Set = new ArithmeticItemSet();

        private static ItemKey Key(string text)
        {
            Assert.IsTrue(ItemKey.TryParse(text, out ItemKey key), "Not a key: " + text);
            return key;
        }

        [Test]
        public void All_HoldsEveryFactInThreeUnknownPositionsAndEveryNumberIsInRange()
        {
            // 231 ordered pairs (a, b) with a + b <= 20, each as an addition and as a subtraction, three unknowns each.
            Assert.AreEqual(231 * 2 * 3, Set.All.Count);
            foreach (ArithmeticItem item in Set.All.Cast<ArithmeticItem>())
            {
                Assert.IsTrue(NumberRange.Contains(item.First), item.Key.ToString());
                Assert.IsTrue(NumberRange.Contains(item.Second), item.Key.ToString());
                Assert.IsTrue(NumberRange.Contains(item.Result), item.Key.ToString());
                Assert.IsTrue(NumberRange.Contains(item.Solution), item.Key.ToString());
                Assert.AreEqual(Activity.Arithmetic, item.Activity);
            }
        }

        [Test]
        public void All_HoldsThreeHundredNinetySixWithinTenItems()
        {
            // 66 ordered pairs with a sum of 10 or less, times two operations, times three unknown positions:
            // the pool the pacing question Q4 in the technical spec counts from.
            Assert.AreEqual(396, Set.All.Count(item => item.Difficulty == Difficulty.WithinTen));
        }

        [Test]
        public void All_KeysAreUnique()
        {
            Assert.AreEqual(Set.All.Count, Set.All.Select(item => item.Key).Distinct().Count());
        }

        [Test]
        public void All_HasTheFourFormsOfAFact_InEveryUnknownPosition()
        {
            var keys = new HashSet<ItemKey>(Set.All.Select(item => item.Key));

            // 3 + 7 = 10, 7 + 3 = 10, 10 - 3 = 7, 10 - 7 = 3
            foreach (string key in new[] { "A:3+7=?", "A:7+3=?", "A:10-3=?", "A:10-7=?" })
            {
                Assert.IsTrue(keys.Contains(Key(key)), key + " is missing");
            }

            foreach (string key in new[] { "A:3+?=10", "A:?+3=10", "A:?+7=10", "A:10-?=3", "A:?-3=7", "A:?-7=3" })
            {
                Assert.IsTrue(keys.Contains(Key(key)), key + " is missing");
            }
        }

        [Test]
        public void All_IsOrderedEasiestFirstThenByKey_AndTheSameOnEveryConstruction()
        {
            for (int i = 1; i < Set.All.Count; i++)
            {
                Item before = Set.All[i - 1];
                Item after = Set.All[i];
                Assert.LessOrEqual(before.Difficulty, after.Difficulty);
                if (before.Difficulty == after.Difficulty)
                {
                    Assert.Less(string.CompareOrdinal(before.Key.ToString(), after.Key.ToString()), 0);
                }
            }

            CollectionAssert.AreEqual(
                Set.All.Select(item => item.Key.ToString()),
                new ArithmeticItemSet().All.Select(item => item.Key.ToString()));
        }

        [Test]
        public void TryFind_RebuildsEveryItemOfTheSetFromItsKey()
        {
            foreach (Item item in Set.All)
            {
                Assert.IsTrue(Set.TryFind(item.Key, out Item found), item.Key.ToString());

                var expected = (ArithmeticItem)item;
                var actual = (ArithmeticItem)found;
                Assert.AreEqual(expected.Key, actual.Key);
                Assert.AreEqual(
                    (expected.First, expected.Operation, expected.Second, expected.Result, expected.Unknown),
                    (actual.First, actual.Operation, actual.Second, actual.Result, actual.Unknown));
                Assert.AreEqual(expected.Difficulty, actual.Difficulty);
            }
        }

        [TestCase("A:8+5=?", 8, Operation.Addition, 5, 13, Unknown.Result)]
        [TestCase("A:8+?=13", 8, Operation.Addition, 5, 13, Unknown.Second)]
        [TestCase("A:?+5=13", 8, Operation.Addition, 5, 13, Unknown.First)]
        [TestCase("A:13-6=?", 13, Operation.Subtraction, 6, 7, Unknown.Result)]
        [TestCase("A:13-?=7", 13, Operation.Subtraction, 6, 7, Unknown.Second)]
        [TestCase("A:?-6=7", 13, Operation.Subtraction, 6, 7, Unknown.First)]
        [TestCase("A:0+0=?", 0, Operation.Addition, 0, 0, Unknown.Result)]
        [TestCase("A:?-20=0", 20, Operation.Subtraction, 20, 0, Unknown.First)]
        public void TryFind_BuildsTheItemTheKeyNames(
            string key, int first, Operation operation, int second, int result, Unknown unknown)
        {
            Assert.IsTrue(Set.TryFind(Key(key), out Item found));

            var item = (ArithmeticItem)found;
            Assert.AreEqual((first, operation, second, result, unknown), (item.First, item.Operation, item.Second, item.Result, item.Unknown));
            Assert.AreEqual(key, item.Key.ToString());
        }

        [TestCase("A:8+5=14", Description = "stated result is not 8 + 5, and no number is missing")]
        [TestCase("A:8+5=13", Description = "nothing is missing, so nothing to ask")]
        [TestCase("A:3-?=7", Description = "the missing number would be -4")]
        [TestCase("A:8+?=5", Description = "the missing number would be -3")]
        [TestCase("A:5-7=?", Description = "the result would be -2")]
        [TestCase("A:15+?=3", Description = "the missing number would be -12")]
        [TestCase("A:?+5=21", Description = "a stated number above 20")]
        [TestCase("A:21-1=?", Description = "a stated number above 20")]
        [TestCase("A:?+30=33", Description = "a stated number above 20")]
        [TestCase("A:18+5=?", Description = "the result would be 23")]
        [TestCase("A:?-5=19", Description = "the missing number would be 24")]
        [TestCase("A:08+5=?", Description = "not the canonical form")]
        [TestCase("A:8+?=?", Description = "two numbers missing")]
        [TestCase("A:?+?=?", Description = "three numbers missing")]
        [TestCase("A:8*5=?", Description = "an operation that is not in the set")]
        [TestCase("A:8+5=?x", Description = "trailing text")]
        [TestCase("A:", Description = "no fact")]
        [TestCase("L:Place:14", Description = "another activity")]
        [TestCase("C:7?12", Description = "another activity")]
        [TestCase("garbage", Description = "no key form")]
        public void TryFind_KeyOfAnItemThatCannotExist_ReturnsFalse(string key)
        {
            Assert.IsTrue(ItemKey.TryParse(key, out ItemKey parsed), "The text must be a well-formed key for this test to mean anything.");

            Assert.IsFalse(Set.TryFind(parsed, out Item item));
            Assert.IsNull(item);
        }

        [Test]
        public void TryFind_DefaultKey_ReturnsFalse()
        {
            Assert.IsFalse(Set.TryFind(default, out _));
        }

        [TestCase(8, 5, Difficulty.CrossingTen)]
        [TestCase(5, 8, Difficulty.CrossingTen)]
        [TestCase(9, 2, Difficulty.CrossingTen)]
        [TestCase(9, 9, Difficulty.CrossingTen)]
        [TestCase(5, 5, Difficulty.WithinTen)]
        [TestCase(0, 10, Difficulty.WithinTen)]
        [TestCase(3, 7, Difficulty.WithinTen)]
        [TestCase(12, 3, Difficulty.BeyondTenWithoutCrossing)]
        [TestCase(3, 12, Difficulty.BeyondTenWithoutCrossing)]
        [TestCase(10, 5, Difficulty.BeyondTenWithoutCrossing)]
        [TestCase(10, 10, Difficulty.BeyondTenWithoutCrossing)]
        public void Difficulty_OfAFact_IsTheSameInAllFourFormsAndAllThreeUnknowns(int a, int b, Difficulty expected)
        {
            int sum = a + b;
            foreach (Unknown unknown in new[] { Unknown.Result, Unknown.First, Unknown.Second })
            {
                Assert.AreEqual(expected, new ArithmeticItem(a, Operation.Addition, b, sum, unknown).Difficulty, a + "+" + b);
                Assert.AreEqual(expected, new ArithmeticItem(b, Operation.Addition, a, sum, unknown).Difficulty, b + "+" + a);
                Assert.AreEqual(expected, new ArithmeticItem(sum, Operation.Subtraction, a, b, unknown).Difficulty, sum + "-" + a);
                Assert.AreEqual(expected, new ArithmeticItem(sum, Operation.Subtraction, b, a, unknown).Difficulty, sum + "-" + b);
            }
        }

        [TestCase(17, 4, Difficulty.BeyondTenWithoutCrossing)]
        [TestCase(13, 6, Difficulty.CrossingTen)]
        [TestCase(13, 3, Difficulty.BeyondTenWithoutCrossing)]
        public void Difficulty_OfASubtraction_FollowsTheFactItBelongsTo(int first, int second, Difficulty expected)
        {
            Assert.AreEqual(expected, new ArithmeticItem(first, Operation.Subtraction, second, first - second, Unknown.Result).Difficulty);
        }

        [Test]
        public void Constructor_NumberOutsideRange_OrFalseFact_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new ArithmeticItem(18, Operation.Addition, 5, 23, Unknown.Result));
            Assert.Throws<System.ArgumentException>(() => new ArithmeticItem(3, Operation.Subtraction, 7, -4, Unknown.Result));
            Assert.Throws<System.ArgumentException>(() => new ArithmeticItem(8, Operation.Addition, 5, 14, Unknown.Result));
        }

        [Test]
        public void Constructor_UndefinedUnknown_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new ArithmeticItem(8, Operation.Addition, 5, 13, (Unknown)7));
        }

        [TestCase("A:8+5=?", 13, true)]
        [TestCase("A:8+5=?", 12, false)]
        [TestCase("A:8+?=13", 5, true)]
        [TestCase("A:?+5=13", 8, true)]
        [TestCase("A:?+5=13", 13, false)]
        [TestCase("A:13-6=?", 7, true)]
        [TestCase("A:13-?=7", 6, true)]
        [TestCase("A:?-6=7", 13, true)]
        [TestCase("A:?-6=7", 7, false)]
        public void IsCorrect_AcceptsOnlyTheMissingNumber(string key, int answer, bool expected)
        {
            Set.TryFind(Key(key), out Item item);

            Assert.AreEqual(expected, item.IsCorrect(Answer.FromNumber(answer)));
        }

        [Test]
        public void IsCorrect_AnAnswerWithNoNumber_IsWrong()
        {
            Set.TryFind(Key("A:8+5=?"), out Item item);

            Assert.IsFalse(item.IsCorrect(default));
        }
    }
}
