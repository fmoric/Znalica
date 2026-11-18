using System.Collections.Generic;
using System.IO;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    /// <summary>An <see cref="IAnswerLog"/> whose next Append throws an <see cref="IOException"/> when <see cref="FailNext"/> is set, as a full or locked disk would.</summary>
    public sealed class FailingAnswerLog : IAnswerLog
    {
        public InMemoryAnswerLog Inner { get; } = new InMemoryAnswerLog();

        public bool FailNext { get; set; }

        public void Append(AnswerRecord record)
        {
            if (FailNext)
            {
                FailNext = false;
                throw new IOException("The disk is full.");
            }

            Inner.Append(record);
        }

        public IReadOnlyList<AnswerRecord> ReadAll()
        {
            return Inner.ReadAll();
        }
    }
}
