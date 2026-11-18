using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>
    /// What a parent sees per activity, worked out from the answer log and never stored. The item schedule says which
    /// items are still weak (box 0 or 1). Dates are read in the offset of <c>now</c>, so a week is a calendar week on
    /// the clock of the PC the parent is sitting at.
    /// </summary>
    public static class ProgressCalculator
    {
        public const int StillMissedMax = 5;
        public const int StillMissedWindowDays = 28;
        public const int WeekCount = 6;

        private const int StillMissedMaxBox = 1;
        private const int DaysInWeek = 7;

        /// <summary>The activities in the order the progress screen shows them.</summary>
        public static readonly IReadOnlyList<Activity> Activities = new[] { Activity.Arithmetic, Activity.Comparison, Activity.NumberLine };

        /// <summary>One report for each of <see cref="Activities"/>, in that order; an activity with no answers gets an empty report.</summary>
        public static IReadOnlyList<ProgressReport> For(IReadOnlyList<AnswerRecord> log, ItemSchedule schedule, DateTimeOffset now)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            return Activities.Select(activity => ForActivity(activity, log.Where(r => r.Activity == activity).ToList(), schedule, now)).ToList();
        }

        private static ProgressReport ForActivity(Activity activity, List<AnswerRecord> records, ItemSchedule schedule, DateTimeOffset now)
        {
            int firstTries = records.Count(r => r.Attempt == AttemptKind.First);
            double? share = firstTries == 0 ? (double?)null : records.Count(r => r.IsRightFirstTime) / (double)firstTries;
            return new ProgressReport(
                activity,
                records.Select(r => r.RoundId).Distinct().Count(),
                share,
                StillMissed(records, schedule, now),
                WeeklyPractice(records, now));
        }

        private static IReadOnlyList<ItemKey> StillMissed(List<AnswerRecord> records, ItemSchedule schedule, DateTimeOffset now)
        {
            DateTimeOffset since = now.AddDays(-StillMissedWindowDays);
            var wrong = new Dictionary<ItemKey, int>();
            foreach (AnswerRecord record in records)
            {
                if (record.Attempt != AttemptKind.First
                    || record.IsCorrect
                    || !TryReadDate(record, out DateTimeOffset at)
                    || at < since
                    || !ItemKey.TryParse(record.ItemKey, out ItemKey key))
                {
                    continue;
                }

                wrong[key] = (wrong.TryGetValue(key, out int count) ? count : 0) + 1;
            }

            // Ties are broken by the key's text, so the same log always gives the same list.
            return wrong
                .Where(pair => schedule.Get(pair.Key) is ItemProgress progress && progress.Box <= StillMissedMaxBox)
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key.ToString(), StringComparer.Ordinal)
                .Take(StillMissedMax)
                .Select(pair => pair.Key)
                .ToList();
        }

        private static IReadOnlyList<WeekPractice> WeeklyPractice(List<AnswerRecord> records, DateTimeOffset now)
        {
            DateTime currentMonday = MondayOf(now.Date);
            var rounds = new HashSet<string>[WeekCount];
            var answers = new int[WeekCount];
            for (int i = 0; i < WeekCount; i++)
            {
                rounds[i] = new HashSet<string>(StringComparer.Ordinal);
            }

            foreach (AnswerRecord record in records)
            {
                if (!TryReadDate(record, out DateTimeOffset at))
                {
                    continue;
                }

                int weeksAgo = (currentMonday - MondayOf(at.ToOffset(now.Offset).Date)).Days / DaysInWeek;
                if (weeksAgo < 0 || weeksAgo >= WeekCount)
                {
                    continue;
                }

                int slot = WeekCount - 1 - weeksAgo;
                rounds[slot].Add(record.RoundId);
                answers[slot]++;
            }

            var weeks = new List<WeekPractice>();
            for (int slot = 0; slot < WeekCount; slot++)
            {
                weeks.Add(new WeekPractice(currentMonday.AddDays(-DaysInWeek * (WeekCount - 1 - slot)), rounds[slot].Count, answers[slot]));
            }

            return weeks;
        }

        private static DateTime MondayOf(DateTime date)
        {
            return date.Date.AddDays(-(((int)date.DayOfWeek + DaysInWeek - 1) % DaysInWeek));
        }

        private static bool TryReadDate(AnswerRecord record, out DateTimeOffset at)
        {
            return DateTimeOffset.TryParse(record.AnsweredAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out at);
        }
    }
}
