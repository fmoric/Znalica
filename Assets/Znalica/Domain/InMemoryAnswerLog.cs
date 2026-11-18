using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>An <see cref="IAnswerLog"/> that keeps the records in memory: for tests.</summary>
    public sealed class InMemoryAnswerLog : IAnswerLog
    {
        private readonly List<AnswerRecord> _records = new List<AnswerRecord>();

        public void Append(AnswerRecord record)
        {
            _records.Add(record ?? throw new ArgumentNullException(nameof(record)));
        }

        public IReadOnlyList<AnswerRecord> ReadAll()
        {
            return _records.ToArray();
        }
    }
}
