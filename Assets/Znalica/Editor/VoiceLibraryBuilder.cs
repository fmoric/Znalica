using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Znalica.Domain;
using Znalica.Presentation;
using Znalica.Storage;

namespace Znalica.Editor
{
    /// <summary>
    /// Regenerates the <see cref="VoiceLibrary"/> asset from the clip files in the voice folder, and writes
    /// VoiceScript.csv, the script for whoever (or whatever) produces the clips. The clip source changes by replacing
    /// files of the same names; no code changes. BuildScript.Build runs this before every build.
    /// </summary>
    public static class VoiceLibraryBuilder
    {
        private const string LibraryPath = VoiceClipFiles.Folder + "/VoiceLibrary.asset";
        private const string ScriptPath = VoiceClipFiles.Folder + "/VoiceScript.csv";

        [MenuItem("Znalica/Rebuild Voice Library")]
        public static void Rebuild()
        {
            AssetDatabase.Refresh();
            WriteLibrary();
            WriteScript();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void WriteLibrary()
        {
            var entries = new List<VoiceLibrary.Entry>();
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { VoiceClipFiles.Folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetDirectoryName(path).Replace('\\', '/') != VoiceClipFiles.Folder
                    || !path.EndsWith(VoiceClipFiles.Extension, StringComparison.Ordinal))
                {
                    Debug.LogWarning("Voice library ignores " + path + ": clips are " + VoiceClipFiles.Extension
                        + " files directly in " + VoiceClipFiles.Folder + ".");
                    continue;
                }

                entries.Add(new VoiceLibrary.Entry
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    Clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path),
                });
            }

            entries.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            var library = AssetDatabase.LoadAssetAtPath<VoiceLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<VoiceLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            library.SetEntries(entries);
            EditorUtility.SetDirty(library);
        }

        /// <summary>One row per whole-sentence clip: the clip name and the Croatian sentence to say.</summary>
        private static void WriteScript()
        {
            PhraseCatalog catalog = PhraseCatalogFile.Read(
                PhraseCatalogFile.PathIn(Application.streamingAssetsPath), new UnityProblemLog());

            var rows = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<PhraseKey, Phrase> entry in catalog.Entries)
            {
                string name = entry.Key.ToString();
                if (entry.Value.Voice.Length == 1 && entry.Value.Voice[0] == name)
                {
                    rows.Add(new KeyValuePair<string, string>(name, entry.Value.Text));
                }
            }

            rows.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

            var csv = new StringBuilder();
            csv.Append("\"Clip\",\"Sentence\"\r\n");
            foreach (KeyValuePair<string, string> row in rows)
            {
                csv.Append(Quote(row.Key)).Append(',').Append(Quote(row.Value)).Append("\r\n");
            }

            File.WriteAllText(ScriptPath, csv.ToString(), new UTF8Encoding(true));
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
