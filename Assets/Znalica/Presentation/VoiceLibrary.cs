using System;
using System.Collections.Generic;
using UnityEngine;

namespace Znalica.Presentation
{
    /// <summary>
    /// Maps a clip name to its AudioClip. Generated, not hand-edited: VoiceLibraryBuilder fills it from the files in
    /// the voice folder, so changing the voice means replacing files of the same names.
    /// </summary>
    public sealed class VoiceLibrary : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string Name;
            public AudioClip Clip;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        private Dictionary<string, AudioClip> _byName;

        public IReadOnlyList<Entry> Entries => _entries;

        public void SetEntries(IEnumerable<Entry> entries)
        {
            _entries = new List<Entry>(entries);
            _byName = null;
        }

        public bool TryGetClip(string name, out AudioClip clip)
        {
            if (_byName == null)
            {
                _byName = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
                foreach (Entry entry in _entries)
                {
                    _byName[entry.Name] = entry.Clip;
                }
            }

            return _byName.TryGetValue(name, out clip) && clip != null;
        }
    }
}
