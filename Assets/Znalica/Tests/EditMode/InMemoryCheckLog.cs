using System;
using System.Collections.Generic;
using System.IO;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    /// <summary>
    /// An <see cref="ICheckLog"/> that keeps the lines in memory: for tests. <see cref="FailOnAppend"/> makes the append
    /// with that number (counted from 1, over the log's life) throw an <see cref="IOException"/> once, as a full disk would.
    /// </summary>
    public sealed class InMemoryCheckLog : ICheckLog
    {
        private readonly List<CheckLine> _lines = new List<CheckLine>();
        private int _appends;

        public int? FailOnAppend { get; set; }

        public void Append(CheckLine line)
        {
            _appends++;
            if (FailOnAppend == _appends)
            {
                throw new IOException("The disk is full.");
            }

            _lines.Add(line ?? throw new ArgumentNullException(nameof(line)));
        }

        public IReadOnlyList<CheckLine> ReadAll()
        {
            return _lines.ToArray();
        }
    }
}
