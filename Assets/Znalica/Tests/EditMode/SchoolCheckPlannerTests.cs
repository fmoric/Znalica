using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class SchoolCheckPlannerTests
    {
        private const int Seeds = 60;

        private static IReadOnlyList<Item> PlanFor(int seed)
        {
            return SchoolCheckPlanner.Plan(new SeededRandom(seed));
        }

        private static IEnumerable<Item> InSection(IReadOnlyList<Item> plan, CheckSection section)
        {
            return plan.Where(item => SchoolCheckPlanner.SectionOf(item) == section);
        }

        [Test]
        public void Plan_HoldsTwentyItems_InTheStatedMix_SectionBySection()
        {
            for (int seed = 0; seed < Seeds; seed++)
            {
                IReadOnlyList<Item> plan = PlanFor(seed);

                Assert.AreEqual(20, plan.Count, "seed " + seed);
                Assert.AreEqual(6, InSection(plan, CheckSection.Result).Count(), "seed " + seed);
                Assert.AreEqual(4, InSection(plan, CheckSection.MissingNumber).Count(), "seed " + seed);
                Assert.AreEqual(4, InSection(plan, CheckSection.Sign).Count(), "seed " + seed);
                Assert.AreEqual(6, InSection(plan, CheckSection.Neighbour).Count(), "seed " + seed);

                // The sections stand together and in this order, so page one (the first ten) is arithmetic only.
                CheckSection[] order = plan.Select(SchoolCheckPlanner.SectionOf).ToArray();
                CollectionAssert.AreEqual(order.OrderBy(section => section).ToArray(), order, "seed " + seed);
                Assert.IsTrue(plan.Take(10).All(item => item is ArithmeticItem), "seed " + seed);
            }
        }

        [Test]
        public void Plan_EveryItemIsInRange_AndCanBeRebuiltFromItsKey()
        {
            var sets = new IItemSet[] { new ArithmeticItemSet(), new ComparisonItemSet(), new NumberLineItemSet() };
            for (int seed = 0; seed < Seeds; seed++)
            {
                foreach (Item item in PlanFor(seed))
                {
                    IEnumerable<int> numbers;
                    if (item is ArithmeticItem arithmetic)
                    {
                        numbers = new[] { arithmetic.First, arithmetic.Second, arithmetic.Result };
                    }
                    else if (item is ComparisonItem comparison)
                    {
                        numbers = comparison.Numbers;
                    }
                    else
                    {
                        var line = (NumberLineItem)item;
                        numbers = new[] { line.Target, line.Solution };
                    }

                    Assert.IsTrue(numbers.All(NumberRange.Contains), item.Key + " has a number outside 0-20, seed " + seed);
                    Item rebuilt = null;
                    Assert.IsTrue(sets.Single(set => set.Activity == item.Activity).TryFind(item.Key, out rebuilt), item.Key.ToString());
                    Assert.AreEqual(item.Key, rebuilt.Key);
                }
            }
        }

        [Test]
        public void Plan_DrawsEachKindEvenlyAcrossItsDifficulties()
        {
            for (int seed = 0; seed < Seeds; seed++)
            {
                IReadOnlyList<Item> plan = PlanFor(seed);

                CollectionAssert.AreEqual(new[] { 2, 2, 2 }, CountByDifficulty(InSection(plan, CheckSection.Result), 3), "result, seed " + seed);
                CollectionAssert.AreEqual(new[] { 2, 2 }, CountByDifficulty(InSection(plan, CheckSection.Sign), 2), "sign, seed " + seed);
                CollectionAssert.AreEqual(new[] { 3, 3 }, CountByDifficulty(InSection(plan, CheckSection.Neighbour), 2), "neighbour, seed " + seed);

                // Four over three difficulties: one each and one more somewhere.
                int[] missing = CountByDifficulty(InSection(plan, CheckSection.MissingNumber), 3);
                CollectionAssert.AreEquivalent(new[] { 1, 1, 2 }, missing, "missing number, seed " + seed);
            }
        }

        [Test]
        public void Plan_TheDifficultyThatGetsTheExtraMissingNumberItem_VariesWithTheSeed()
        {
            var extra = new HashSet<int>();
            for (int seed = 0; seed < Seeds; seed++)
            {
                extra.Add(Array.IndexOf(CountByDifficulty(InSection(PlanFor(seed), CheckSection.MissingNumber), 3), 2));
            }

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, extra);
        }

        [Test]
        public void Plan_TheNumberLineItems_AreOnlyBeforeOrAfter_WithTheAnswerTyped()
        {
            for (int seed = 0; seed < Seeds; seed++)
            {
                var items = InSection(PlanFor(seed), CheckSection.Neighbour).Cast<NumberLineItem>().ToList();

                Assert.AreEqual(6, items.Count);
                Assert.IsTrue(items.All(item => item.Task == NumberLineTask.Before || item.Task == NumberLineTask.After), "seed " + seed);
                Assert.IsTrue(items.All(item => item.Key.ToString().StartsWith("L:Before:") || item.Key.ToString().StartsWith("L:After:")));
            }
        }

        [Test]
        public void Plan_TheSignItemsAreWithNumbers_AndTheArithmeticUnknownsAreSplitAsStated()
        {
            for (int seed = 0; seed < Seeds; seed++)
            {
                IReadOnlyList<Item> plan = PlanFor(seed);

                Assert.IsTrue(
                    InSection(plan, CheckSection.Sign).Cast<ComparisonItem>()
                        .All(item => item.Task == ComparisonTask.ChooseSign && item.Representation == Representation.Numbers),
                    "seed " + seed);
                Assert.IsTrue(InSection(plan, CheckSection.Result).Cast<ArithmeticItem>().All(item => item.Unknown == Unknown.Result));
                Assert.IsTrue(InSection(plan, CheckSection.MissingNumber).Cast<ArithmeticItem>().All(item => item.Unknown != Unknown.Result));
            }
        }

        [Test]
        public void Plan_NeverRunsOutOfItems_OverManySeeds_AndAlwaysReturnsTwenty()
        {
            for (int seed = 0; seed < 2000; seed++)
            {
                Assert.AreEqual(20, PlanFor(seed).Count, "seed " + seed);
            }
        }

        [Test]
        public void Plan_NoItemAndNoFactIsOnTheWorksheetTwice()
        {
            for (int seed = 0; seed < Seeds; seed++)
            {
                IReadOnlyList<Item> plan = PlanFor(seed);

                Assert.AreEqual(20, plan.Select(item => item.Key.ToString()).Distinct().Count(), "seed " + seed);
                Assert.AreEqual(20, plan.Select(item => item.FactId).Distinct().Count(), "seed " + seed);
            }
        }

        [Test]
        public void Plan_WithTheSameSeed_IsTheSame_AndOtherSeedsDiffer()
        {
            string[] first = PlanFor(11).Select(item => item.Key.ToString()).ToArray();

            CollectionAssert.AreEqual(first, PlanFor(11).Select(item => item.Key.ToString()).ToArray());
            Assert.IsTrue(Enumerable.Range(0, 10).Any(seed => !first.SequenceEqual(PlanFor(seed + 100).Select(item => item.Key.ToString()))));
        }

        [Test]
        public void SectionOf_AnItemTheWorksheetHasNoPlaceFor_Throws()
        {
            Assert.Throws<ArgumentException>(() => SchoolCheckPlanner.SectionOf(NumberLineItem.Place(5)));
            Assert.Throws<ArgumentException>(() => SchoolCheckPlanner.SectionOf(NumberLineItem.Jump(5, 3)));
            Assert.Throws<ArgumentException>(() => SchoolCheckPlanner.SectionOf(ComparisonItem.PutInOrder(new[] { 3, 9, 14 })));
        }

        /// <summary>How many items there are of each difficulty, indexed by its value (WithinTen, BeyondTenWithoutCrossing, CrossingTen).</summary>
        private static int[] CountByDifficulty(IEnumerable<Item> items, int levels)
        {
            int[] counts = new int[levels];
            foreach (Item item in items)
            {
                counts[(int)item.Difficulty]++;
            }

            return counts;
        }
    }
}
