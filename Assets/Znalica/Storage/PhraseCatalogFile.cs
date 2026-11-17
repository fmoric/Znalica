using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// Reads Hr.json, the loose text file under StreamingAssets, into a <see cref="PhraseCatalog"/>. The file is an
    /// object wrapping the list, because JsonUtility cannot parse a top-level array. A broken file never stops the
    /// game: each problem is reported and the entries that are fine are kept.
    /// </summary>
    public static class PhraseCatalogFile
    {
        private const string Folder = "Text";
        private const string FileName = "Hr.json";

        [Serializable]
        private sealed class FileDto
        {
            public EntryDto[] Entries;
        }

        [Serializable]
        private sealed class EntryDto
        {
            public string Key;
            public string Text;
            public string[] Voice;
        }

        /// <summary>Where Hr.json is, given <c>Application.streamingAssetsPath</c>.</summary>
        public static string PathIn(string streamingAssetsPath)
        {
            return Path.Combine(streamingAssetsPath, Folder, FileName);
        }

        public static PhraseCatalog Read(string path, IProblemLog problems)
        {
            if (!File.Exists(path))
            {
                problems.Report("Text file not found: " + path);
                return new PhraseCatalog(new KeyValuePair<PhraseKey, Phrase>[0], problems);
            }

            return Parse(File.ReadAllText(path, Encoding.UTF8), problems);
        }

        /// <summary>A key is its exact member name: Enum.TryParse alone would also take "0" or "5" as a key.</summary>
        private static bool TryParseKey(string name, out PhraseKey key)
        {
            return Enum.TryParse(name, out key) && Enum.GetName(typeof(PhraseKey), key) == name;
        }

        public static PhraseCatalog Parse(string json, IProblemLog problems)
        {
            var entries = new Dictionary<PhraseKey, Phrase>();

            FileDto file = null;
            bool reported = false;
            try
            {
                file = JsonUtility.FromJson<FileDto>(json);
            }
            catch (ArgumentException exception)
            {
                problems.Report("Text file is not valid JSON: " + exception.Message);
                reported = true;
            }

            if (file == null && !reported)
            {
                problems.Report("Text file is empty.");
            }

            if (file != null && file.Entries == null)
            {
                problems.Report("Text file has no Entries list.");
            }

            foreach (EntryDto entry in file?.Entries ?? new EntryDto[0])
            {
                if (entry == null || !TryParseKey(entry.Key, out PhraseKey key))
                {
                    problems.Report("Text file has an entry with an unknown key: " + entry?.Key);
                    continue;
                }

                if (entries.ContainsKey(key))
                {
                    problems.Report("Text file has the key " + key + " twice; the last one is used.");
                }

                entries[key] = new Phrase(entry.Text, entry.Voice);
            }

            return new PhraseCatalog(entries, problems);
        }
    }
}
