using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>The append-only record of every answer. An implementation returns from Append only once the record is kept.</summary>
    public interface IAnswerLog
    {
        void Append(AnswerRecord record);

        IReadOnlyList<AnswerRecord> ReadAll();
    }
}
