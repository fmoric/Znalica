using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ComparisonItemSetTests
    {
        private static readonly ComparisonItemSet Set = new ComparisonItemSet();

        private static ItemKey Key(string text)
        {
            Assert.IsTrue(ItemKey.TryParse(text, out ItemKey key), text + " is not even a well-formed key.");
            return key;
        }

        private static bool Find(string key, out Item item)
        {
            return Set.TryFind(Key(key), out item);
        }

        private static IEnumerable<int> NumbersOf(Item item)
        {
            return ((ComparisonItem)item).Numbers;
        }

        [Test]
        public void All_HoldsEveryPairOfNumbersIn0To20_InBothRepresentations()
        {
            Assert.AreEqual(21 * 21 * 2, Set.All.Count);
            Assert.AreEqual(Set.All.Count, Set.All.Select(item => item.Key.ToString()).Distinct().Count(), "A key is used twice.");
            Assert.IsTrue(Set.All.All(item => NumbersOf(item).All(NumberRange.Contains)), "A number is outside 0-20.");
            Assert.IsTrue(Set.All.All(item => ((ComparisonItem)item).Task == ComparisonTask.ChooseSign), "Ordering items are not in All.");
            Assert.IsTrue(Set.All.Any(item => item.Key.ToString() == "C:7?12"));
            Assert.IsTrue(Set.All.Any(item => item.Key.ToString() == "C:G12?7"));
            Assert.IsTrue(Set.All.Any(item => item.Key.ToString() == "C:G0?0"));
            Assert.IsTrue(Set.All.Any(item => item.Key.ToString() == "C:20?20"));
        }

        [Test]
        public void All_IsInOrderOfDifficultyThenKey_AndTheEasyLevelHoldsOnlyNumbersUpToTen()
        {
            var sorted = Set.All
                .OrderBy(item => item.Difficulty)
                .ThenBy(item => item.Key.ToString(), System.StringComparer.Ordinal)
                .Select(item => item.Key.ToString());

            CollectionAssert.AreEqual(sorted.ToList(), Set.All.Select(item => item.Key.ToString()).ToList());
            Assert.AreEqual(11 * 11 * 2, Set.All.Count(item => item.Difficulty == Difficulty.WithinTen));
            Assert.IsTrue(Set.All.Where(item => item.Difficulty == Difficulty.WithinTen).All(item => NumbersOf(item).All(n => n <= 10)));
            Assert.IsTrue(Set.All.Where(item => item.Difficulty == Difficulty.BeyondTenWithoutCrossing).All(item => NumbersOf(item).Any(n => n > 10)));
        }

        [TestCase(3, 7, Sign.Less)]
        [TestCase(12, 7, Sign.Greater)]
        [TestCase(9, 9, Sign.Equal)]
        [TestCase(0, 20, Sign.Less)]
        [TestCase(0, 0, Sign.Equal)]
        public void ChooseSign_IsRightForTheSignOfThePair_AndWrongForTheOtherTwo(int left, int right, Sign expected)
        {
            foreach (Representation representation in new[] { Representation.Numbers, Representation.Groups })
            {
                ComparisonItem item = ComparisonItem.ChooseSign(representation, left, right);

                foreach (Sign sign in new[] { Sign.Less, Sign.Greater, Sign.Equal })
                {
                    Assert.AreEqual(sign == expected, item.IsCorrect(Answer.FromSign(sign)), representation + " " + left + " ? " + right + " answered " + sign);
                }

                Assert.IsFalse(item.IsCorrect(Answer.FromNumber(left)), "A number is not a sign.");
                Assert.IsFalse(item.IsCorrect(Answer.FromOrder(new[] { left, right })), "An order is not a sign.");
            }
        }

        [Test]
        public void PutInOrder_HasASortedKey_WhateverOrderTheNumbersWereGivenIn()
        {
            Assert.AreEqual("O:3,9,14", ComparisonItem.PutInOrder(new[] { 14, 3, 9 }).Key.ToString());
            Assert.AreEqual("O:3,9,14", ComparisonItem.PutInOrder(new[] { 3, 9, 14 }).Key.ToString());
            Assert.AreEqual("O:0,1,2,19,20", ComparisonItem.PutInOrder(new[] { 20, 0, 19, 1, 2 }).Key.ToString());
        }

        [Test]
        public void PutInOrder_IsRightOnlyForTheNumbersSmallestFirst()
        {
            ComparisonItem item = ComparisonItem.PutInOrder(new[] { 14, 3, 9 });

            Assert.IsTrue(item.IsCorrect(Answer.FromOrder(new[] { 3, 9, 14 })));
            Assert.IsFalse(item.IsCorrect(Answer.FromOrder(new[] { 14, 9, 3 })));
            Assert.IsFalse(item.IsCorrect(Answer.FromOrder(new[] { 3, 14, 9 })));
            Assert.IsFalse(item.IsCorrect(Answer.FromOrder(new[] { 3, 9 })), "Too few numbers.");
            Assert.IsFalse(item.IsCorrect(Answer.FromSign(Sign.Less)));
            Assert.IsFalse(item.IsCorrect(Answer.FromNumber(3)));
        }

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void PutInOrder_ShowsTheSameNumbersInAnOrderThatIsNotTheAnswer(int count)
        {
            ComparisonItem item = ComparisonItem.PutInOrder(Enumerable.Range(2, count).Select(n => n * 3));

            CollectionAssert.AreEquivalent(item.Numbers, item.ShownNumbers);
            CollectionAssert.AreNotEqual(item.Numbers, item.ShownNumbers, "The numbers are shown already in order.");
        }

        [Test]
        public void TryFind_RebuildsAnOrderingItemFromItsKey_WithThoseNumbers()
        {
            Assert.IsTrue(Find("O:3,9,14", out Item item));

            ComparisonItem ordering = (ComparisonItem)item;
            Assert.AreEqual(ComparisonTask.PutInOrder, ordering.Task);
            CollectionAssert.AreEqual(new[] { 3, 9, 14 }, ordering.Numbers);
            Assert.AreEqual("O:3,9,14", ordering.Key.ToString());
            Assert.AreEqual(Activity.Comparison, ordering.Activity);
        }

        [TestCase("O:3,3,9", Description = "A repeated number")]
        [TestCase("O:9,3", Description = "Two numbers, descending")]
        [TestCase("O:3,9", Description = "Two numbers")]
        [TestCase("O:9,3,14", Description = "Not ascending")]
        [TestCase("O:3,9,21", Description = "A number above 20")]
        [TestCase("O:0,1,2,3,4,5", Description = "Six numbers")]
        [TestCase("O:03,9,14", Description = "A leading zero")]
        [TestCase("O:3,9,", Description = "An empty number")]
        [TestCase("O:", Description = "No numbers")]
        public void TryFind_IsFalseForAnOrderingKeyThatCannotExist(string key)
        {
            Assert.IsFalse(Find(key, out Item item));
            Assert.IsNull(item);
        }

        [TestCase("C:7?21")]
        [TestCase("C:G7?21")]
        [TestCase("C:07?12")]
        [TestCase("C:7?")]
        [TestCase("C:7?12?3")]
        [TestCase("C:G")]
        [TestCase("C:H7?12")]
        [TestCase("A:8+5=?")]
        [TestCase("L:Place:14")]
        [TestCase("X:7?12")]
        public void TryFind_IsFalseForASignKeyThatCannotExistAndForAnotherActivitysKey(string key)
        {
            Assert.IsFalse(Find(key, out Item item));
            Assert.IsNull(item);
        }

        [Test]
        public void TryFind_RebuildsEveryItemOfTheSetFromItsKey()
        {
            foreach (Item item in Set.All)
            {
                Assert.IsTrue(Find(item.Key.ToString(), out Item found), item.Key.ToString());
                Assert.AreEqual(item.Key, found.Key);
                CollectionAssert.AreEqual(NumbersOf(item), NumbersOf(found));
                Assert.AreEqual(((ComparisonItem)item).Representation, ((ComparisonItem)found).Representation);
            }
        }

        [Test]
        public void FactId_OfASignItem_IsThePairOfNumbers_WhicheverSideAndHoweverShown()
        {
            string fact = ComparisonItem.ChooseSign(Representation.Numbers, 7, 12).FactId;

            Assert.AreEqual(fact, ComparisonItem.ChooseSign(Representation.Numbers, 12, 7).FactId);
            Assert.AreEqual(fact, ComparisonItem.ChooseSign(Representation.Groups, 7, 12).FactId);
            Assert.AreEqual(fact, ComparisonItem.ChooseSign(Representation.Groups, 12, 7).FactId);
            Assert.AreNotEqual(fact, ComparisonItem.ChooseSign(Representation.Numbers, 7, 13).FactId);
            Assert.AreEqual(11 * 12 / 2, Set.All.Where(i => i.Difficulty == Difficulty.WithinTen).Select(i => i.FactId).Distinct().Count());
        }

        [Test]
        public void FactId_OfAnOrderingItem_IsItsOwnKey()
        {
            ComparisonItem item = ComparisonItem.PutInOrder(new[] { 3, 9, 14 });

            Assert.AreEqual("O:3,9,14", item.FactId);
        }

        [Test]
        public void Difficulty_IsWithinTenOnlyWhenEveryNumberIsTenOrLess()
        {
            Assert.AreEqual(Difficulty.WithinTen, ComparisonItem.ChooseSign(Representation.Numbers, 10, 3).Difficulty);
            Assert.AreEqual(Difficulty.BeyondTenWithoutCrossing, ComparisonItem.ChooseSign(Representation.Numbers, 3, 11).Difficulty);
            Assert.AreEqual(Difficulty.WithinTen, ComparisonItem.PutInOrder(new[] { 0, 5, 10 }).Difficulty);
            Assert.AreEqual(Difficulty.BeyondTenWithoutCrossing, ComparisonItem.PutInOrder(new[] { 0, 5, 11 }).Difficulty);
        }

        [Test]
        public void Constructors_RefuseANumberOutsideTheRangeAndABadOrderingSet()
        {
            Assert.Throws<System.ArgumentException>(() => ComparisonItem.ChooseSign(Representation.Numbers, 21, 3));
            Assert.Throws<System.ArgumentException>(() => ComparisonItem.ChooseSign(Representation.Groups, 3, -1));
            Assert.Throws<System.ArgumentException>(() => ComparisonItem.PutInOrder(new[] { 3, 9, 21 }));
            Assert.Throws<System.ArgumentException>(() => ComparisonItem.PutInOrder(new[] { 3, 3, 9 }));
            Assert.Throws<System.ArgumentException>(() => ComparisonItem.PutInOrder(new[] { 3, 9 }));
            Assert.Throws<System.ArgumentException>(() => ComparisonItem.PutInOrder(new[] { 1, 2, 3, 4, 5, 6 }));
        }
    }
}
