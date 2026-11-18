using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// When each item is due, derived from the answer log and never stored: <see cref="Build"/> replays the log and
    /// <see cref="Apply"/> moves one item for one answer. Only the first ask of an item in a round moves it
    /// (<see cref="AttemptKind.First"/>); a Retry or a Return does not.
    /// <list type="bullet">
    /// <item>Wrong: box 0, due in the next session.</item>
    /// <item>Right: one box up, to at most <see cref="MaxBox"/>, due after that box's interval in sessions.</item>
    /// <item>An item with no answer is new: it has no entry, and counts as box 0 when first answered.</item>
    /// </list>
    /// </summary>
    public sealed class ItemSchedule
    {
        public const int MaxBox = 4;

        /// <summary>Sessions until an item is due again, by the box it has just reached.</summary>
        private static readonly int[] IntervalsInSessions = { 1, 1, 2, 4, 8 };

        private readonly Dictionary<ItemKey, ItemProgress> _progress = new Dictionary<ItemKey, ItemProgress>();

        public static ItemSchedule Build(IEnumerable<AnswerRecord> log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var schedule = new ItemSchedule();
            foreach (AnswerRecord record in log)
            {
                schedule.Apply(record);
            }

            return schedule;
        }

        /// <summary>Every item that has been answered, with its progress; the order is not defined.</summary>
        public IEnumerable<KeyValuePair<ItemKey, ItemProgress>> Entries => _progress;

        /// <summary>The item's progress, or null for a new item.</summary>
        public ItemProgress? Get(ItemKey key)
        {
            return _progress.TryGetValue(key, out ItemProgress progress) ? progress : (ItemProgress?)null;
        }

        /// <summary>Moves the item the record names. A record with a key that does not parse is skipped.</summary>
        public void Apply(AnswerRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (record.Attempt != AttemptKind.First || !ItemKey.TryParse(record.ItemKey, out ItemKey key))
            {
                return;
            }

            if (!record.IsCorrect)
            {
                _progress[key] = new ItemProgress(0, record.SessionNumber + IntervalsInSessions[0]);
                return;
            }

            int box = Math.Min((_progress.TryGetValue(key, out ItemProgress before) ? before.Box : 0) + 1, MaxBox);
            _progress[key] = new ItemProgress(box, record.SessionNumber + IntervalsInSessions[box]);
        }
    }
}
