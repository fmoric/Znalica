using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// The append-only record of a profile's school-like checks, kept apart from the <see cref="IAnswerLog"/>: a check
    /// neither moves the schedule nor counts in the game's progress. An implementation returns from Append only once the
    /// line is kept.
    /// </summary>
    public interface ICheckLog
    {
        void Append(CheckLine line);

        IReadOnlyList<CheckLine> ReadAll();
    }
}
