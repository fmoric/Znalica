using System;
using System.Collections.Generic;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>
    /// Plans a round of Veće, manje, jednako: the <see cref="RoundPlanner"/>'s round over the sign items (due ordering
    /// items come back through the schedule, since <see cref="ComparisonItemSet.TryFind"/> builds them from their keys),
    /// with new sign items swapped for freshly drawn ordering items until the round holds <see cref="OrderingPerRound"/>
    /// ordering items. An item that is not new is never swapped, so a round of due items keeps them all.
    /// </summary>
    public static class ComparisonRoundPlanner
    {
        /// <summary>How many ordering items a round aims for. A proposal, like the planner's caps.</summary>
        public const int OrderingPerRound = 2;

        private const int MaxDrawAttempts = 50;

        public static IReadOnlyList<Item> Plan(IItemSet set, ItemSchedule schedule, int sessionNumber, IRandom random)
        {
            List<Item> round = RoundPlanner.Plan(set, schedule, sessionNumber, random).ToList();
            int ordering = round.Count(IsOrdering);
            int largest = RoundPlanner.CurrentLevel(set, schedule) == Difficulty.WithinTen ? 10 : NumberRange.Max;

            for (int i = round.Count - 1; i >= 0 && ordering < OrderingPerRound; i--)
            {
                if (IsOrdering(round[i]) || schedule.Get(round[i].Key).HasValue)
                {
                    continue;
                }

                for (int attempt = 0; attempt < MaxDrawAttempts; attempt++)
                {
                    ComparisonItem drawn = Draw(random, largest);
                    if (!schedule.Get(drawn.Key).HasValue && round.All(item => !item.Key.Equals(drawn.Key)))
                    {
                        round[i] = drawn;
                        ordering++;
                        break;
                    }
                }
            }

            return round;
        }

        /// <summary>Three to five different numbers from 0 to <paramref name="largest"/>.</summary>
        public static ComparisonItem Draw(IRandom random, int largest)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            int count = ComparisonItem.MinOrderCount + random.Next(ComparisonItem.MaxOrderCount - ComparisonItem.MinOrderCount + 1);
            var pool = new List<int>();
            for (int number = NumberRange.Min; number <= largest; number++)
            {
                pool.Add(number);
            }

            var numbers = new List<int>();
            while (numbers.Count < count)
            {
                int index = random.Next(pool.Count);
                numbers.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return ComparisonItem.PutInOrder(numbers);
        }

        private static bool IsOrdering(Item item)
        {
            return item is ComparisonItem comparison && comparison.Task == ComparisonTask.PutInOrder;
        }
    }
}
