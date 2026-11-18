using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// A JSON Lines file that is only ever appended to: the way the answer log and the check log are kept. <see cref="Append"/>
    /// returns only after the line has been written and flushed to disk (also the operating system's buffers), so a closed or
    /// killed app has lost at most the line being written. <see cref="Read"/> skips a line that does not read back (the one a
    /// power cut can leave half-written), reports it and never rewrites the file.
    /// </summary>
    internal static class JsonLinesFile
    {
        private const byte LineEnd = (byte)'\n';

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        /// <summary>Turns the line's data object into the value read from it; false when a field is missing or not what its type allows.</summary>
        public delegate bool TryConvert<TDto, T>(TDto dto, out T result);

        public static void Append(string path, string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                // A file that ends mid-line (a half-written last record) must not swallow this record into the same line.
                bool endsMidLine = false;
                if (stream.Length > 0)
                {
                    stream.Seek(-1, SeekOrigin.End);
                    endsMidLine = stream.ReadByte() != LineEnd;
                }

                stream.Seek(0, SeekOrigin.End);
                byte[] bytes = Utf8.GetBytes((endsMidLine ? "\n" : string.Empty) + json + "\n");
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        public static List<T> Read<TDto, T>(string path, IProblemLog problems, TryConvert<TDto, T> convert)
            where TDto : class
        {
            var records = new List<T>();
            if (!File.Exists(path))
            {
                return records;
            }

            string text;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

                if (TryParse(line, convert, out T record))
                {
                    records.Add(record);
                }
                else
                {
                    problems.Report("Skipped an unreadable line " + (i + 1) + " of " + path + ".");
                }
            }

            return records;
        }

        private static bool TryParse<TDto, T>(string line, TryConvert<TDto, T> convert, out T record)
            where TDto : class
        {
            record = default;
            if (line[0] != '{' || line[line.Length - 1] != '}')
            {
                return false;
            }

            try
            {
                TDto dto = JsonUtility.FromJson<TDto>(line);
                return dto != null && convert(dto, out record);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
