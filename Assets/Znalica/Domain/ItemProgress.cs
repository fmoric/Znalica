namespace Znalica.Domain
{
    /// <summary>Where one item stands in the schedule: its box and the session it is next due in.</summary>
    public readonly struct ItemProgress
    {
        public ItemProgress(int box, int dueSession)
        {
            Box = box;
            DueSession = dueSession;
        }

        /// <summary>0 after a miss, up to <see cref="ItemSchedule.MaxBox"/> after right answers in a row.</summary>
        public int Box { get; }

        /// <summary>The first session in which the item is due again.</summary>
        public int DueSession { get; }
    }
}
