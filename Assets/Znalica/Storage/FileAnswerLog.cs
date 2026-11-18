using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// The answer log on disk: JSON Lines, one record per line, appended. <see cref="Append"/> returns only after the
    /// line has been written and flushed to disk (also the operating system's buffers), so a closed or killed app has lost
    /// at most the answer being written. <see cref="ReadAll"/> skips a line that does not read back (the one a power cut can
    /// leave half-written), reports it and never rewrites the file.
    /// </summary>
    public sealed class FileAnswerLog : IAnswerLog
    {
        private const byte LineEnd = (byte)'\n';

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

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

            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            string line = JsonUtility.ToJson(AnswerRecordDto.From(record)) + "\n";
            using (var stream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                // A file that ends mid-line (a half-written last record) must not swallow this record into the same line.
                bool endsMidLine = false;
                if (stream.Length > 0)
                {
                    stream.Seek(-1, SeekOrigin.End);
                    endsMidLine = stream.ReadByte() != LineEnd;
                }

                stream.Seek(0, SeekOrigin.End);
                byte[] bytes = Utf8.GetBytes((endsMidLine ? "\n" : string.Empty) + line);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        public IReadOnlyList<AnswerRecord> ReadAll()
        {
            var records = new List<AnswerRecord>();
            if (!File.Exists(_path))
            {
                return records;
            }

            string text;
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Utf8))
            {
                text = reader.ReadToEnd();
            }

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (TryParse(line, out AnswerRecord record))
                {
                    records.Add(record);
                }
                else
                {
                    _problems.Report("Skipped an unreadable line " + (i + 1) + " of " + _path + ".");
                }
            }

            return records;
        }

        private static bool TryParse(string line, out AnswerRecord record)
        {
            record = null;
            if (line[0] != '{' || line[line.Length - 1] != '}')
            {
                return false;
            }

            try
            {
                AnswerRecordDto dto = JsonUtility.FromJson<AnswerRecordDto>(line);
                return dto != null && dto.TryToRecord(out record);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
