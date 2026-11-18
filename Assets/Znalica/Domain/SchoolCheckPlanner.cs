using System;
using System.Collections.Generic;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>
    /// Plans the school-like check: 20 items for a parent to see what the child knows outside the game, drawn without
    /// looking at the schedule. Six arithmetic items with the result unknown, four with a missing number, four sign items
    /// with numbers and six number-line items that ask for the number before or after one. Each kind is drawn evenly across
    /// the difficulties it has, and no fact is on the worksheet twice. The items come out section by section, so the first
    /// ten (the first page) are arithmetic and the last ten are signs and number-line strips.
    /// </summary>
    public static class SchoolCheckPlanner
    {
        public const int ResultCount = 6;
        public const int MissingNumberCount = 4;
        public const int SignCount = 4;
        public const int NeighbourCount = 6;
        public const int ItemCount = ResultCount + MissingNumberCount + SignCount + NeighbourCount;

        /// <summary>Which section of the worksheet an item of the check belongs to.</summary>
        /// <exception cref="ArgumentException">An item no check section holds.</exception>
        public static CheckSection SectionOf(Item item)
        {
            if (item is ArithmeticItem arithmetic)
            {
                return arithmetic.Unknown == Unknown.Result ? CheckSection.Result : CheckSection.MissingNumber;
            }

            if (item is ComparisonItem comparison && comparison.Task == ComparisonTask.ChooseSign)
            {
                return CheckSection.Sign;
            }

            if (item is NumberLineItem numberLine && (numberLine.Task == NumberLineTask.Before || numberLine.Task == NumberLineTask.After))
            {
                return CheckSection.Neighbour;
            }

            throw new ArgumentException("The check has no section for the item " + item.Key + ".", nameof(item));
        }

        public static IReadOnlyList<Item> Plan(IRandom random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            IEnumerable<Item> arithmetic = new ArithmeticItemSet().All;
            IEnumerable<Item> numbers = new ComparisonItemSet().All
                .Where(item => ((ComparisonItem)item).Task == ComparisonTask.ChooseSign && ((ComparisonItem)item).Representation == Representation.Numbers);
            IEnumerable<Item> neighbours = new NumberLineItemSet().All.Where(item => IsNeighbourTask((NumberLineItem)item));

            var usedFacts = new HashSet<string>();
            var plan = new List<Item>();
            plan.AddRange(Draw(arithmetic.Where(item => ((ArithmeticItem)item).Unknown == Unknown.Result), ResultCount, random, usedFacts));
            plan.AddRange(Draw(arithmetic.Where(item => ((ArithmeticItem)item).Unknown != Unknown.Result), MissingNumberCount, random, usedFacts));
            plan.AddRange(Draw(numbers, SignCount, random, usedFacts));
            plan.AddRange(Draw(neighbours, NeighbourCount, random, usedFacts));
            return plan;
        }

        private static bool IsNeighbourTask(NumberLineItem item)
        {
            return item.Task == NumberLineTask.Before || item.Task == NumberLineTask.After;
        }

        /// <summary>
        /// <paramref name="count"/> items of the pool, the same number from each difficulty the pool has; when the count does
        /// not divide, the difficulties that get one more are drawn at random. Within a difficulty the draw is random, skipping
        /// a fact already in <paramref name="usedFacts"/>. The result is shuffled.
        /// </summary>
        private static List<Item> Draw(IEnumerable<Item> pool, int count, IRandom random, HashSet<string> usedFacts)
        {
            List<IGrouping<Difficulty, Item>> levels = pool.GroupBy(item => item.Difficulty).OrderBy(group => group.Key).ToList();
            int[] quota = Enumerable.Repeat(count / levels.Count, levels.Count).ToArray();
            foreach (int level in Pick(Enumerable.Range(0, levels.Count).ToList(), count % levels.Count, random))
            {
                quota[level]++;
            }

            var drawn = new List<Item>();
            for (int level = 0; level < levels.Count; level++)
            {
                drawn.AddRange(Pick(levels[level].ToList(), quota[level], random, item => usedFacts.Add(item.FactId)));
            }

            RoundPlanner.Shuffle(drawn, random);
            return drawn;
        }

        /// <summary>Takes <paramref name="count"/> distinct entries of the list at random, each only if <paramref name="accept"/> says so; emptying the list first is an error.</summary>
        private static List<T> Pick<T>(List<T> candidates, int count, IRandom random, Func<T, bool> accept = null)
        {
            var picked = new List<T>();
            while (picked.Count < count)
            {
                if (candidates.Count == 0)
                {
                    throw new InvalidOperationException("The pool holds too few items to draw " + count + ".");
                }

                int index = random.Next(candidates.Count);
                T candidate = candidates[index];
                candidates[index] = candidates[candidates.Count - 1];
                candidates.RemoveAt(candidates.Count - 1);
                if (accept == null || accept(candidate))
                {
                    picked.Add(candidate);
                }
            }

            return picked;
        }
    }
}
