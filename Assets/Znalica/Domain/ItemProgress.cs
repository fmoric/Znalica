namespace Znalica.Domain
{
    /// <summary>Where one item stands in the schedule: its box and the session it is next due in.</summary>
    public readonly struct ItemProgress
    {
        public ItemProgress(int box, int dueSession, int lastSession)
        {
            Box = box;
            DueSession = dueSession;
            LastSession = lastSession;
        }

        /// <summary>0 after a miss, up to <see cref="ItemSchedule.MaxBox"/> after right answers in a row.</summary>
        public int Box { get; }

        /// <summary>The first session in which the item is due again.</summary>
        public int DueSession { get; }

        /// <summary>The session of the answer that last moved the item: an item is asked at most once in a session.</summary>
        public int LastSession { get; }
    }
}
