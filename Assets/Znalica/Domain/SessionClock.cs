using System;

namespace Znalica.Domain
{
    /// <summary>
    /// How long the picked profile has been playing. It starts when a profile is picked and starts again when another is
    /// picked; once <see cref="BreakAfter"/> has passed, <see cref="BreakDue"/> says a break is worth suggesting. It only
    /// answers; nothing is locked (FS:45).
    /// </summary>
    public sealed class SessionClock
    {
        /// <summary>The session length after which a break is suggested (the spec's "about 10 minutes").</summary>
        public static readonly TimeSpan BreakAfter = TimeSpan.FromMinutes(10);

        private readonly IClock _clock;
        private DateTimeOffset? _started;

        public SessionClock(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>Starts the session now; a session already running starts over.</summary>
        public void Start()
        {
            _started = _clock.Now;
        }

        /// <summary>True when a session is running and has lasted <see cref="BreakAfter"/> or more.</summary>
        public bool BreakDue => _started.HasValue && _clock.Now - _started.Value >= BreakAfter;
    }
}
