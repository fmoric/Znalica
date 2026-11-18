using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class NumberLineItemSetTests
    {
        private static readonly NumberLineItemSet Set = new NumberLineItemSet();

        private static bool Find(string key, out Item item)
        {
            Assert.IsTrue(ItemKey.TryParse(key, out ItemKey parsed), key + " is not even a well-formed key.");
            return Set.TryFind(parsed, out item);
        }

        private static IEnumerable<NumberLineItem> Items(NumberLineTask task)
        {
            return Set.All.Cast<NumberLineItem>().Where(item => item.Task == task);
        }

        [Test]
        public void All_HoldsEveryNumberAndEveryNeighbourAndEveryJumpInRange_WithEachKeyOnce()
        {
            Assert.AreEqual(Set.All.Count, Set.All.Select(item => item.Key.ToString()).Distinct().Count(), "A key is used twice.");
            CollectionAssert.AreEqual(Enumerable.Range(0, 21), Items(NumberLineTask.Place).Select(i => i.Target).OrderBy(n => n).ToList());
            CollectionAssert.AreEqual(Enumerable.Range(1, 20), Items(NumberLineTask.Before).Select(i => i.Target).OrderBy(n => n).ToList());
            CollectionAssert.AreEqual(Enumerable.Range(0, 20), Items(NumberLineTask.After).Select(i => i.Target).OrderBy(n => n).ToList());

            // A jump of 1 to 10 either way from each of the 21 numbers, as long as it lands in 0-20.
            int jumps = 0;
            for (int start = 0; start <= 20; start++)
            {
                for (int step = -10; step <= 10; step++)
                {
                    jumps += step != 0 && start + step >= 0 && start + step <= 20 ? 1 : 0;
                }
            }

            Assert.AreEqual(jumps, Items(NumberLineTask.Jump).Count());
            Assert.IsTrue(Set.All.All(item => item.Activity == Activity.NumberLine));
        }

        [Test]
        public void EveryNumberInEveryItem_AndEveryAnswer_IsIn0To20()
        {
            foreach (NumberLineItem item in Set.All.Cast<NumberLineItem>())
            {
                Assert.IsTrue(NumberRange.Contains(item.Target), item.Key + " names " + item.Target);
                Assert.IsTrue(NumberRange.Contains(item.Solution), item.Key + " has the answer " + item.Solution);
            }
        }

        [Test]
        public void BeforeAndAfter_StopWhereThereIsNoNeighbourInRange()
        {
            Assert.IsFalse(Items(NumberLineTask.Before).Any(i => i.Target == 0), "Nothing is before 0.");
            Assert.IsFalse(Items(NumberLineTask.After).Any(i => i.Target == 20), "Nothing is after 20.");
            Assert.AreEqual(0, NumberLineItem.Before(1).Solution);
            Assert.AreEqual(20, NumberLineItem.After(19).Solution);
            Assert.AreEqual(13, NumberLineItem.Before(14).Solution);
            Assert.AreEqual(15, NumberLineItem.After(14).Solution);
        }

        [Test]
        public void EveryJump_LandsInRange_AndIsBetween1And10Long()
        {
            foreach (NumberLineItem jump in Items(NumberLineTask.Jump))
            {
                int landing = jump.Target + jump.Step;
                Assert.IsTrue(landing >= 0 && landing <= 20, jump.Key + " lands on " + landing);
                Assert.AreEqual(landing, jump.Solution);
                Assert.IsTrue(System.Math.Abs(jump.Step) >= 1 && System.Math.Abs(jump.Step) <= 10, jump.Key + " is " + jump.Step + " long");
            }

            Assert.AreEqual("L:Jump:6+3", NumberLineItem.Jump(6, 3).Key.ToString());
            Assert.AreEqual("L:Jump:9-3", NumberLineItem.Jump(9, -3).Key.ToString());
        }

        [TestCase("L:Before:0", Description = "Nothing before 0")]
        [TestCase("L:After:20", Description = "Nothing after 20")]
        [TestCase("L:Jump:18+5", Description = "Lands on 23")]
        [TestCase("L:Jump:6+11", Description = "A jump of 11")]
        [TestCase("L:Jump:6-7", Description = "Lands below 0")]
        [TestCase("L:Jump:6+0", Description = "A jump of 0")]
        [TestCase("L:Jump:6-0", Description = "A jump of 0 back")]
        [TestCase("L:Jump:21-1", Description = "Starts outside the range")]
        [TestCase("L:Place:21", Description = "A number above 20")]
        [TestCase("L:Place:07", Description = "A leading zero")]
        [TestCase("L:Place:+7", Description = "A sign")]
        [TestCase("L:Jump:", Description = "A jump with nothing after the task")]
        [TestCase("L:Place:", Description = "No number")]
        [TestCase("L:Place", Description = "No number part")]
        [TestCase("L:Place:7:1", Description = "An extra part")]
        [TestCase("L:place:7", Description = "The task in lower case")]
        [TestCase("L:1:5", Description = "A task written as a number")]
        [TestCase("L:Race:5", Description = "No such task")]
        [TestCase("L:Jump:+3", Description = "A jump without a start")]
        [TestCase("L:Jump:6", Description = "A jump without a step")]
        [TestCase("L:Jump:5+", Description = "A sign with no length")]
        [TestCase("L:Jump:+", Description = "A sign alone")]
        [TestCase("L:Jump:+5", Description = "A length with no start")]
        [TestCase("L:Jump:6*3", Description = "A jump with no sign")]
        [TestCase("L:Jump:6+3+1", Description = "Two signs")]
        [TestCase("A:8+5=?", Description = "Another activity")]
        [TestCase("C:7?12", Description = "Another activity")]
        [TestCase("O:3,9,14", Description = "Another activity")]
        public void TryFind_IsFalseForAKeyThatCannotExist(string key)
        {
            Assert.IsFalse(Find(key, out Item item));
            Assert.IsNull(item);
        }

        [TestCase("L:Place:0")]
        [TestCase("L:Place:20")]
        [TestCase("L:Before:1")]
        [TestCase("L:Before:20")]
        [TestCase("L:After:0")]
        [TestCase("L:After:19")]
        [TestCase("L:Jump:0+10")]
        [TestCase("L:Jump:20-10")]
        [TestCase("L:Jump:17+3")]
        public void TryFind_BuildsAnItemAtTheEdgeOfWhatCanExist(string key)
        {
            Assert.IsTrue(Find(key, out Item item));
            Assert.AreEqual(key, item.Key.ToString());
        }

        [Test]
        public void TryFind_RebuildsEveryItemOfTheSetFromItsKey()
        {
            foreach (NumberLineItem item in Set.All.Cast<NumberLineItem>())
            {
                Assert.IsTrue(Find(item.Key.ToString(), out Item found), item.Key.ToString());
                var rebuilt = (NumberLineItem)found;
                Assert.AreEqual(item.Key, rebuilt.Key);
                Assert.AreEqual((item.Task, item.Target, item.Step), (rebuilt.Task, rebuilt.Target, rebuilt.Step));
            }
        }

        [Test]
        public void All_IsInOrderOfDifficultyThenKey_AndTheEasyLevelHoldsOnlyNumbersUpToTen()
        {
            var sorted = Set.All
                .OrderBy(item => item.Difficulty)
                .ThenBy(item => item.Key.ToString(), System.StringComparer.Ordinal)
                .Select(item => item.Key.ToString());

            CollectionAssert.AreEqual(sorted.ToList(), Set.All.Select(item => item.Key.ToString()).ToList());
            foreach (NumberLineItem item in Set.All.Cast<NumberLineItem>())
            {
                bool allUpToTen = item.Target <= 10 && item.Solution <= 10;
                Assert.AreEqual(allUpToTen ? Difficulty.WithinTen : Difficulty.BeyondTenWithoutCrossing, item.Difficulty, item.Key.ToString());
            }

            Assert.AreEqual(Difficulty.BeyondTenWithoutCrossing, NumberLineItem.After(10).Difficulty, "Its answer is 11.");
            Assert.AreEqual(Difficulty.WithinTen, NumberLineItem.Before(10).Difficulty);
        }

        [Test]
        public void IsCorrect_IsTheNumberAskedFor_AndNothingElse()
        {
            Assert.IsTrue(NumberLineItem.Place(14).IsCorrect(Answer.FromNumber(14)));
            Assert.IsFalse(NumberLineItem.Place(14).IsCorrect(Answer.FromNumber(15)));
            Assert.IsTrue(NumberLineItem.Before(14).IsCorrect(Answer.FromNumber(13)));
            Assert.IsFalse(NumberLineItem.Before(14).IsCorrect(Answer.FromNumber(15)));
            Assert.IsTrue(NumberLineItem.After(14).IsCorrect(Answer.FromNumber(15)));
            Assert.IsFalse(NumberLineItem.After(14).IsCorrect(Answer.FromNumber(13)));
            Assert.IsTrue(NumberLineItem.Jump(6, 3).IsCorrect(Answer.FromNumber(9)));
            Assert.IsFalse(NumberLineItem.Jump(6, 3).IsCorrect(Answer.FromNumber(3)));
            Assert.IsTrue(NumberLineItem.Jump(9, -3).IsCorrect(Answer.FromNumber(6)));
            Assert.IsFalse(NumberLineItem.Place(14).IsCorrect(Answer.FromSign(Sign.Less)), "A sign is not a number.");
            Assert.IsFalse(NumberLineItem.Place(14).IsCorrect(Answer.FromOrder(new[] { 14 })), "An order is not a number.");
        }

        [Test]
        public void FactId_JoinsTheFormsThatAskAboutTheSameTwoNumbers()
        {
            Assert.AreEqual(NumberLineItem.Before(14).FactId, NumberLineItem.After(13).FactId, "The neighbours 13 and 14.");
            Assert.AreNotEqual(NumberLineItem.Before(14).FactId, NumberLineItem.After(14).FactId);
            Assert.AreEqual(NumberLineItem.Jump(6, 3).FactId, NumberLineItem.Jump(9, -3).FactId, "The jump between 6 and 9, either way.");
            Assert.AreNotEqual(NumberLineItem.Jump(6, 3).FactId, NumberLineItem.Jump(6, 4).FactId);
            Assert.AreNotEqual(NumberLineItem.Before(14).FactId, NumberLineItem.Jump(13, 1).FactId, "A neighbour and a jump of 1 are different facts.");
            Assert.AreEqual("L:Place:14", NumberLineItem.Place(14).FactId, "Placing a number is its own fact.");

            // The within-ten level: 11 numbers to place, 10 neighbour pairs, 55 pairs of numbers to jump between.
            Assert.AreEqual(11 + 10 + 55, Set.All.Where(i => i.Difficulty == Difficulty.WithinTen).Select(i => i.FactId).Distinct().Count());
        }

        [Test]
        public void ItemsOfOneFact_ShareTheirDifficulty()
        {
            foreach (IGrouping<string, Item> fact in Set.All.GroupBy(item => item.FactId))
            {
                Assert.AreEqual(1, fact.Select(item => item.Difficulty).Distinct().Count(), fact.Key);
            }
        }

        [Test]
        public void Constructors_RefuseWhatTheKeysRefuse()
        {
            Assert.Throws<System.ArgumentException>(() => NumberLineItem.Place(21));
            Assert.Throws<System.ArgumentException>(() => NumberLineItem.Place(-1));
            Assert.Throws<System.ArgumentException>(() => NumberLineItem.Before(0));
            Assert.Throws<System.ArgumentException>(() => NumberLineItem.After(20));
            Assert.Throws<System.ArgumentException>(() => NumberLineItem.Jump(18, 5));
            Assert.Throws<System.ArgumentException>(() => NumberLineItem.Jump(6, 11));
            Assert.Throws<System.ArgumentException>(() => NumberLineItem.Jump(6, 0));
        }
    }
}
