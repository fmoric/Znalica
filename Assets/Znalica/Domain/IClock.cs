using System;

namespace Znalica.Domain
{
    /// <summary>The time, injected so that tests do not depend on the real clock.</summary>
    public interface IClock
    {
        DateTimeOffset Now { get; }
    }
}
