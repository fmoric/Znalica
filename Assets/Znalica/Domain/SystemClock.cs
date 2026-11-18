using System;

namespace Znalica.Domain
{
    /// <summary>The real time. Tests use a fixed clock instead.</summary>
    public sealed class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
