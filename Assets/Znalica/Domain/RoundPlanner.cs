using System;
using System.Collections.Generic;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>
    /// Plans a round of ten items from the item set and the profile's schedule:
    /// <list type="number">
    /// <item>Due items (<see cref="ItemProgress.DueSession"/> not after the session), lowest box first, then longest
    /// due: at most <see cref="DueCap"/>, so the items missed last time lead.</item>
    /// <item>New items from the <see cref="CurrentLevel"/>: at most <see cref="NewCap"/>.</item>
    /// <item>Fill: items not yet due, lowest box first (a due item over the cap is not fill).</item>
    /// <item>If the round is still short (a profile with few items answered), more new items from the current level.</item>
    /// </list>
    /// then shuffled, with no item twice. An item already answered in this session (<see cref="ItemProgress.LastSession"/>)
    /// is never planned again, so it moves at most one box in a session, however many rounds are played.
    /// Ties are broken at random, so the same seed gives the same round.
    /// </summary>
    public static class RoundPlanner
    {
        public const int RoundSize = 10;

        /// <summary>The most due items in one round.</summary>
        public const int DueCap = 6;

        /// <summary>The most new items in one round, before a short round is topped up.</summary>
        public const int NewCap = 4;

        /// <summary>The box from which a fact counts as known when a level is promoted.</summary>
        public const int KnownBox = 3;

        /// <summary>The share of a level's facts that must be known before new items come from the next level.</summary>
        public const int PromotionPercent = 80;

        public static IReadOnlyList<Item> Plan(IItemSet set, ItemSchedule schedule, int sessionNumber, IRandom random)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var chosen = new List<Item>(RoundSize);
            var taken = new HashSet<ItemKey>();

            // Scheduled items that the set can still build, in a fixed order before the random tie-break.
            var scheduled = new List<ScheduledItem>();
            foreach (KeyValuePair<ItemKey, ItemProgress> entry in schedule.Entries.OrderBy(e => e.Key.ToString(), StringComparer.Ordinal))
            {
                if (entry.Value.LastSession != sessionNumber && set.TryFind(entry.Key, out Item item))
                {
                    scheduled.Add(new ScheduledItem(item, entry.Value));
                }
            }

            Shuffle(scheduled, random);
            List<ScheduledItem> byBox = scheduled.OrderBy(s => s.Progress.Box).ThenBy(s => s.Progress.DueSession).ToList();

            foreach (ScheduledItem due in byBox.Where(s => s.Progress.DueSession <= sessionNumber).Take(DueCap))
            {
                Add(chosen, taken, due.Item);
            }

            List<Item> unseen = UnseenOfCurrentLevel(set, schedule);
            Shuffle(unseen, random);
            foreach (Item item in unseen.Take(Math.Min(NewCap, RoundSize - chosen.Count)))
            {
                Add(chosen, taken, item);
            }

            foreach (ScheduledItem notDue in byBox.Where(s => s.Progress.DueSession > sessionNumber).Take(RoundSize - chosen.Count))
            {
                Add(chosen, taken, notDue.Item);
            }

            foreach (Item item in unseen.Where(i => !taken.Contains(i.Key)).Take(RoundSize - chosen.Count))
            {
                Add(chosen, taken, item);
            }

            Shuffle(chosen, random);
            return chosen;
        }

        /// <summary>
        /// The easiest level in which fewer than <see cref="PromotionPercent"/> percent of the facts are known (have an
        /// item in box <see cref="KnownBox"/> or higher), or the hardest level if every level is at least that far.
        /// Null for an empty set.
        /// </summary>
        public static Difficulty? CurrentLevel(IItemSet set, ItemSchedule schedule)
        {
            if (set == null)
            {
                throw new ArgumentNullException(nameof(set));
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            Difficulty? current = null;
            foreach (IGrouping<Difficulty, Item> level in set.All.GroupBy(item => item.Difficulty).OrderBy(g => g.Key))
            {
                current = level.Key;
                var facts = new HashSet<string>();
                var known = new HashSet<string>();
                foreach (Item item in level)
                {
                    facts.Add(item.FactId);
                    ItemProgress? progress = schedule.Get(item.Key);
                    if (progress.HasValue && progress.Value.Box >= KnownBox)
                    {
                        known.Add(item.FactId);
                    }
                }

                if (known.Count * 100 < facts.Count * PromotionPercent)
                {
                    break;
                }
            }

            return current;
        }

        private static List<Item> UnseenOfCurrentLevel(IItemSet set, ItemSchedule schedule)
        {
            Difficulty? level = CurrentLevel(set, schedule);
            return set.All.Where(item => item.Difficulty == level && !schedule.Get(item.Key).HasValue).ToList();
        }

        private static void Add(List<Item> chosen, HashSet<ItemKey> taken, Item item)
        {
            if (chosen.Count < RoundSize && taken.Add(item.Key))
            {
                chosen.Add(item);
            }
        }

        /// <summary>Fisher-Yates shuffle in place; the same seed gives the same order. Also used by the school-like check's draw.</summary>
        internal static void Shuffle<T>(List<T> items, IRandom random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T swap = items[i];
                items[i] = items[j];
                items[j] = swap;
            }
        }

        private readonly struct ScheduledItem
        {
            public ScheduledItem(Item item, ItemProgress progress)
            {
                Item = item;
                Progress = progress;
            }

            public Item Item { get; }

            public ItemProgress Progress { get; }
        }
    }
}
