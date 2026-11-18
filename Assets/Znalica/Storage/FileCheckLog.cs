using System;
using System.Collections.Generic;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// A profile's school-like checks on disk (<c>Checks.jsonl</c>), kept in the same way as the answer log
    /// (<see cref="JsonLinesFile"/>) and in a file of its own, so a check never reaches <c>Answers.jsonl</c>.
    /// </summary>
    public sealed class FileCheckLog : ICheckLog
    {
        private readonly string _path;
        private readonly IProblemLog _problems;

        public FileCheckLog(string path, IProblemLog problems)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public void Append(CheckLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            JsonLinesFile.Append(_path, JsonUtility.ToJson(CheckLineDto.From(line)));
        }

        public IReadOnlyList<CheckLine> ReadAll()
        {
            return JsonLinesFile.Read<CheckLineDto, CheckLine>(
                _path, _problems, (CheckLineDto dto, out CheckLine line) => dto.TryToLine(out line));
        }
    }
}
