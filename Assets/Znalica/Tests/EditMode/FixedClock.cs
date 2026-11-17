using System;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    /// <summary>An <see cref="IClock"/> that says the time it was given.</summary>
    public sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset now)
        {
            Now = now;
        }

        public DateTimeOffset Now { get; set; }
    }
}
