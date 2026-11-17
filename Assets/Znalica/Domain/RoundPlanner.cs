using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// Plans a round. Without an item schedule yet every item is new, so the round is the easiest items of the set,
    /// drawn at random and shuffled, with no item twice. The schedule's due items and the cap on new items come with it.
    /// </summary>
    public static class RoundPlanner
    {
        public const int RoundSize = 10;

        public static IReadOnlyList<Item> Plan(IItemSet set, IRandom random)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            // All is ordered easiest first: take the easiest level that is not used up, drawing at random inside it.
            var chosen = new List<Item>(RoundSize);
            int start = 0;
            while (start < set.All.Count && chosen.Count < RoundSize)
            {
                int end = start;
                while (end < set.All.Count && set.All[end].Difficulty == set.All[start].Difficulty)
                {
                    end++;
                }

                var level = new List<Item>();
                for (int i = start; i < end; i++)
                {
                    level.Add(set.All[i]);
                }

                Shuffle(level, random);
                chosen.AddRange(level.GetRange(0, Math.Min(level.Count, RoundSize - chosen.Count)));
                start = end;
            }

            Shuffle(chosen, random);
            return chosen;
        }

        private static void Shuffle(List<Item> items, IRandom random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Item swap = items[i];
                items[i] = items[j];
                items[j] = swap;
            }
        }
    }
}
