using System;
using System.Collections.Generic;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
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
