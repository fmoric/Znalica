using System;
using System.Collections.Generic;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// The answer log on disk: JSON Lines, one record per line, appended through <see cref="JsonLinesFile"/>, which returns
    /// from an append only after the line is on disk and skips, reports and never rewrites a line that does not read back.
    /// </summary>
    public sealed class FileAnswerLog : IAnswerLog
    {
        private readonly string _path;
        private readonly IProblemLog _problems;

        public FileAnswerLog(string path, IProblemLog problems)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public void Append(AnswerRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            JsonLinesFile.Append(_path, JsonUtility.ToJson(AnswerRecordDto.From(record)));
        }

        public IReadOnlyList<AnswerRecord> ReadAll()
        {
            return JsonLinesFile.Read<AnswerRecordDto, AnswerRecord>(
                _path, _problems, (AnswerRecordDto dto, out AnswerRecord record) => dto.TryToRecord(out record));
        }
    }
}
