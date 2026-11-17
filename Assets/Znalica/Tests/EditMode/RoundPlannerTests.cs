using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class RoundPlannerTests
    {
        private sealed class ListItemSet : IItemSet
        {
            public ListItemSet(IReadOnlyList<Item> all)
            {
                All = all;
            }

            public Activity Activity => Activity.Arithmetic;

            public IReadOnlyList<Item> All { get; }

            public bool TryFind(ItemKey key, out Item item)
            {
                item = All.FirstOrDefault(candidate => candidate.Key.Equals(key));
                return item != null;
            }
        }

        private static ArithmeticItem Sum(int a, int b)
        {
            return new ArithmeticItem(a, Operation.Addition, b, a + b, Unknown.Result);
        }

        [Test]
        public void Plan_GivesTenDistinctItemsOfTheSet()
        {
            var set = new ArithmeticItemSet();

            IReadOnlyList<Item> round = RoundPlanner.Plan(set, new SeededRandom(1));

            Assert.AreEqual(RoundPlanner.RoundSize, round.Count);
            Assert.AreEqual(round.Count, round.Select(item => item.Key).Distinct().Count(), "no item twice");
            Assert.IsTrue(round.All(item => set.TryFind(item.Key, out _)));
        }

        [Test]
        public void Plan_StartsWithTheEasiestItems()
        {
            IReadOnlyList<Item> round = RoundPlanner.Plan(new ArithmeticItemSet(), new SeededRandom(2));

            Assert.IsTrue(round.All(item => item.Difficulty == Difficulty.WithinTen));
        }

        [Test]
        public void Plan_WhenTheEasiestLevelHasTooFew_FillsFromTheNextLevel()
        {
            var easy = new List<Item> { Sum(1, 2), Sum(2, 2), Sum(3, 2) };
            var harder = new List<Item>();
            for (int a = 12; a <= 14; a++)
            {
                for (int b = 0; b <= 5; b++)
                {
                    harder.Add(Sum(a, b));
                }
            }

            IReadOnlyList<Item> round = RoundPlanner.Plan(new ListItemSet(easy.Concat(harder).ToList()), new SeededRandom(3));

            Assert.AreEqual(10, round.Count);
            Assert.AreEqual(3, round.Count(item => item.Difficulty == Difficulty.WithinTen), "all the easy ones are in");
            Assert.AreEqual(7, round.Count(item => item.Difficulty == Difficulty.BeyondTenWithoutCrossing));
        }

        [Test]
        public void Plan_SetWithFewerThanTenItems_GivesThemAll()
        {
            var set = new ListItemSet(new List<Item> { Sum(1, 1), Sum(1, 2), Sum(1, 3) });

            Assert.AreEqual(3, RoundPlanner.Plan(set, new SeededRandom(4)).Count);
        }

        [Test]
        public void Plan_SameSeedGivesTheSameRound_DifferentSeedsGiveDifferentOnes()
        {
            var set = new ArithmeticItemSet();

            string Keys(int seed) => string.Join(" ", RoundPlanner.Plan(set, new SeededRandom(seed)).Select(item => item.Key.ToString()));

            Assert.AreEqual(Keys(5), Keys(5));
            Assert.AreNotEqual(Keys(5), Keys(6));
        }

        [Test]
        public void Plan_EveryNumberOfEveryPlannedItemIsInRange_OverManySeeds()
        {
            var set = new ArithmeticItemSet();
            for (int seed = 0; seed < 200; seed++)
            {
                foreach (ArithmeticItem item in RoundPlanner.Plan(set, new SeededRandom(seed)).Cast<ArithmeticItem>())
                {
                    Assert.IsTrue(
                        NumberRange.Contains(item.First) && NumberRange.Contains(item.Second) && NumberRange.Contains(item.Result),
                        item.Key.ToString());
                }
            }
        }

        [Test]
        public void Plan_Shuffles_SoTheOrderIsNotTheSetsOrder()
        {
            var set = new ArithmeticItemSet();
            int sorted = 0;
            for (int seed = 0; seed < 50; seed++)
            {
                List<string> keys = RoundPlanner.Plan(set, new SeededRandom(seed)).Select(item => item.Key.ToString()).ToList();
                if (keys.SequenceEqual(keys.OrderBy(key => key, System.StringComparer.Ordinal)))
                {
                    sorted++;
                }
            }

            Assert.Less(sorted, 5, "ten shuffled keys are almost never already in key order");
        }
    }
}
