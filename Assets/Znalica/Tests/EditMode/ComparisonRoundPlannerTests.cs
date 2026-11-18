using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ComparisonRoundPlannerTests
    {
        private const int Session = 4;

        private static readonly ComparisonItemSet Set = new ComparisonItemSet();

        private static IEnumerable<ComparisonItem> Ordering(IEnumerable<Item> round)
        {
            return round.Cast<ComparisonItem>().Where(item => item.Task == ComparisonTask.PutInOrder);
        }

        [Test]
        public void ANewProfilesRound_HoldsTenItems_TwoOfThemOrderingItems_AllInRangeAndWithinTen()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                IReadOnlyList<Item> round = ComparisonRoundPlanner.Plan(Set, new ItemSchedule(), Session, new SeededRandom(seed));

                Assert.AreEqual(RoundPlanner.RoundSize, round.Count, "seed " + seed);
                Assert.AreEqual(ComparisonRoundPlanner.OrderingPerRound, Ordering(round).Count(), "seed " + seed);
                Assert.AreEqual(round.Count, round.Select(item => item.Key.ToString()).Distinct().Count(), "An item is in the round twice, seed " + seed);
                Assert.IsTrue(round.Cast<ComparisonItem>().All(item => item.Numbers.All(n => NumberRange.Contains(n) && n <= 10)), "seed " + seed);
                Assert.IsTrue(round.All(item => item.Activity == Activity.Comparison));
            }
        }

        [Test]
        public void ADrawnOrderingItem_HoldsThreeToFiveDifferentNumbers_FromZeroToTheLimit_AllSizesOccur()
        {
            var random = new SeededRandom(7);
            var sizes = new HashSet<int>();

            for (int i = 0; i < 300; i++)
            {
                ComparisonItem item = ComparisonRoundPlanner.Draw(random, 20);

                sizes.Add(item.Numbers.Count);
                Assert.IsTrue(item.Numbers.All(NumberRange.Contains));
                Assert.AreEqual(item.Numbers.Count, item.Numbers.Distinct().Count());
                Assert.IsTrue(ItemKey.TryParse(item.Key.ToString(), out ItemKey key) && Set.TryFind(key, out _), "The drawn item cannot be rebuilt from its key.");
            }

            CollectionAssert.AreEquivalent(new[] { 3, 4, 5 }, sizes);
        }

        [Test]
        public void ADrawnOrderingItem_StaysWithinTheLimit()
        {
            var random = new SeededRandom(3);

            for (int i = 0; i < 200; i++)
            {
                Assert.IsTrue(ComparisonRoundPlanner.Draw(random, 10).Numbers.All(n => n <= 10));
            }
        }

        [Test]
        public void ADueOrderingItem_ComesBackThroughTheScheduleAsItself()
        {
            var schedule = new ItemSchedule();
            schedule.Apply(TestRecords.Make(sessionNumber: Session - 1, itemKey: "O:3,9,14", isCorrect: false));

            IReadOnlyList<Item> round = ComparisonRoundPlanner.Plan(Set, schedule, Session, new SeededRandom(1));

            Assert.IsTrue(round.Any(item => item.Key.ToString() == "O:3,9,14"), "The missed ordering item is not in the round.");
            Assert.AreEqual(ComparisonRoundPlanner.OrderingPerRound, Ordering(round).Count());
            Assert.AreEqual(1, round.Count(item => item.Key.ToString() == "O:3,9,14"));
        }

        [Test]
        public void DueItems_AreNeverSwappedForADrawnOrderingItem()
        {
            var schedule = new ItemSchedule();
            var due = new List<string>();
            for (int left = 0; left < 6; left++)
            {
                string key = "C:" + left + "?" + (left + 1);
                schedule.Apply(TestRecords.Make(sessionNumber: Session - 1, itemKey: key, isCorrect: false));
                due.Add(key);
            }

            IReadOnlyList<Item> round = ComparisonRoundPlanner.Plan(Set, schedule, Session, new SeededRandom(5));

            foreach (string key in due)
            {
                Assert.AreEqual(1, round.Count(item => item.Key.ToString() == key), key + " was swapped out.");
            }
        }

        [Test]
        public void AFullRoundOfDueOrderingItems_IsLeftAsItIs()
        {
            var schedule = new ItemSchedule();
            for (int low = 0; low < 6; low++)
            {
                schedule.Apply(TestRecords.Make(sessionNumber: Session - 1, itemKey: "O:" + low + "," + (low + 7) + "," + (low + 9), isCorrect: false));
            }

            IReadOnlyList<Item> round = ComparisonRoundPlanner.Plan(Set, schedule, Session, new SeededRandom(2));

            Assert.AreEqual(6, Ordering(round).Count(item => schedule.Get(item.Key).HasValue));
            Assert.GreaterOrEqual(Ordering(round).Count(), 6);
        }

        [Test]
        public void ThePlannerReadsASchedulesSignItemsOnlyOfThisActivity()
        {
            var schedule = new ItemSchedule();
            schedule.Apply(TestRecords.Make(sessionNumber: Session - 1, itemKey: "A:8+5=?", isCorrect: false));

            IReadOnlyList<Item> round = ComparisonRoundPlanner.Plan(Set, schedule, Session, new SeededRandom(1));

            Assert.IsTrue(round.All(item => item.Activity == Activity.Comparison));
        }
    }
}
