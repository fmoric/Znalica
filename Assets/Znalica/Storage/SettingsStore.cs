using System;
using System.IO;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>Reads and writes <c>Settings.json</c>. A missing or unreadable file gives the defaults and never stops the game.</summary>
    public sealed class SettingsStore
    {
        private readonly DataFolder _folder;
        private readonly IProblemLog _problems;

        public SettingsStore(DataFolder folder, IProblemLog problems)
        {
            _folder = folder ?? throw new ArgumentNullException(nameof(folder));
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        public Settings Load()
        {
            string path = _folder.SettingsPath;
            if (!File.Exists(path))
            {
                return new Settings();
            }

            try
            {
                Settings settings = JsonUtility.FromJson<Settings>(File.ReadAllText(path));
                if (settings == null)
                {
                    throw new ArgumentException("The file is empty.");
                }

                settings.VoiceVolume = VoiceVolume.Clamp(settings.VoiceVolume);
                return settings;
            }
            catch (ArgumentException exception)
            {
                _problems.Report("Settings file " + path + " is unreadable, defaults are used: " + exception.Message);
                return new Settings();
            }
        }

        public void Save(Settings settings)
        {
            var clamped = new Settings { VoiceVolume = VoiceVolume.Clamp(settings.VoiceVolume) };
            AtomicFileWriter.Write(_folder.SettingsPath, JsonUtility.ToJson(clamped));
        }
    }
}
