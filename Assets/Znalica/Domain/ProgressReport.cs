using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>How a child is doing in one activity, as <see cref="ProgressCalculator"/> works it out from the answer log.</summary>
    public sealed class ProgressReport
    {
        public ProgressReport(
            Activity activity,
            int roundsPlayed,
            double? firstTimeRightShare,
            IReadOnlyList<ItemKey> stillMissed,
            IReadOnlyList<WeekPractice> weeklyPractice)
        {
            Activity = activity;
            RoundsPlayed = roundsPlayed;
            FirstTimeRightShare = firstTimeRightShare;
            StillMissed = stillMissed;
            WeeklyPractice = weeklyPractice;
        }

        public Activity Activity { get; }

        /// <summary>Distinct round ids of the activity in the whole log.</summary>
        public int RoundsPlayed { get; }

        /// <summary>Right first tries over all first tries, 0.0 to 1.0; null when there is no first try yet.</summary>
        public double? FirstTimeRightShare { get; }

        /// <summary>Up to <see cref="ProgressCalculator.StillMissedMax"/> items, the most often missed first.</summary>
        public IReadOnlyList<ItemKey> StillMissed { get; }

        /// <summary>The last <see cref="ProgressCalculator.WeekCount"/> calendar weeks, the oldest first and the current one last.</summary>
        public IReadOnlyList<WeekPractice> WeeklyPractice { get; }
    }
}
