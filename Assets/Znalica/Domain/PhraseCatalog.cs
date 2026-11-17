using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Znalica.Domain
{
    /// <summary>
    /// All phrases of the game, by <see cref="PhraseKey"/>. A key with no entry never stops the game: the key name is
    /// shown instead and the problem is reported to the <see cref="IProblemLog"/>.
    /// </summary>
    public sealed class PhraseCatalog
    {
        private static readonly Regex TextSlot = new Regex(@"\{([A-Za-z0-9_]+)\}", RegexOptions.Compiled);

        private readonly Dictionary<PhraseKey, Phrase> _phrases = new Dictionary<PhraseKey, Phrase>();
        private readonly IProblemLog _problems;

        public PhraseCatalog(IEnumerable<KeyValuePair<PhraseKey, Phrase>> entries, IProblemLog problems)
        {
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
            foreach (KeyValuePair<PhraseKey, Phrase> entry in entries)
            {
                _phrases[entry.Key] = entry.Value;
            }
        }

        public IEnumerable<KeyValuePair<PhraseKey, Phrase>> Entries => _phrases;

        public bool Contains(PhraseKey key)
        {
            return _phrases.ContainsKey(key);
        }

        /// <summary>The phrase for the key; a missing key gives a phrase whose text is the key name, and is reported.</summary>
        public Phrase Get(PhraseKey key)
        {
            if (_phrases.TryGetValue(key, out Phrase phrase))
            {
                return phrase;
            }

            _problems.Report("Phrase catalog has no entry for " + key + ".");
            return new Phrase(key.ToString(), null);
        }

        /// <summary>The text with each <c>{Name}</c> replaced by the argument of that name.</summary>
        public string Text(PhraseKey key, IReadOnlyDictionary<string, string> args = null)
        {
            return TextSlot.Replace(Get(key).Text, match =>
            {
                string name = match.Groups[1].Value;
                if (args != null && args.TryGetValue(name, out string value))
                {
                    return value;
                }

                _problems.Report("Phrase " + key + " needs an argument named " + name + ".");
                return match.Value;
            });
        }

        /// <summary>
        /// The clip names to play, in order. A number slot becomes the bare-numeral clip of its argument
        /// (<c>"8"</c> gives <c>Number08</c>); a slot whose argument is missing or outside 0-20 is reported and left out.
        /// </summary>
        public IReadOnlyList<string> VoiceClips(PhraseKey key, IReadOnlyDictionary<string, string> args = null)
        {
            var clips = new List<string>();
            foreach (string item in Get(key).Voice)
            {
                if (!PhraseSlot.TryParse(item, out string slotName))
                {
                    clips.Add(item);
                    continue;
                }

                if (args != null
                    && args.TryGetValue(slotName, out string value)
                    && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
                    && NumberRange.Contains(number))
                {
                    clips.Add(PhraseSlot.NumberClipName(number));
                }
                else
                {
                    _problems.Report("Phrase " + key + " has no number 0-20 for its voice slot " + slotName + ".");
                }
            }

            return clips;
        }
    }
}
