using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// The completeness check: every <see cref="PhraseKey"/> has a text, and every clip a phrase speaks exists.
    /// It sees clip names, not grammar: whether a number slot stands alone, as the spoken-forms rule requires, is for
    /// the Croatian speaker who reads the texts.
    /// </summary>
    public static class PhraseCatalogCheck
    {
        /// <summary>The problems found, one line each; empty when the catalog is complete for these clips.</summary>
        public static IReadOnlyList<string> Missing(PhraseCatalog catalog, IEnumerable<string> clipNames)
        {
            var available = new HashSet<string>(clipNames, StringComparer.Ordinal);
            var problems = new List<string>();

            foreach (PhraseKey key in Enum.GetValues(typeof(PhraseKey)))
            {
                if (!catalog.Contains(key))
                {
                    problems.Add("No entry for " + key + ".");
                    continue;
                }

                Phrase phrase = catalog.Get(key);
                if (string.IsNullOrWhiteSpace(phrase.Text))
                {
                    problems.Add("Empty text for " + key + ".");
                }

                foreach (string item in phrase.Voice)
                {
                    IEnumerable<string> needed = PhraseSlot.TryParse(item, out _)
                        ? PhraseSlot.AllNumberClipNames()
                        : new[] { item };
                    foreach (string clip in needed)
                    {
                        if (!available.Contains(clip))
                        {
                            problems.Add("No clip " + clip + " for " + key + ".");
                        }
                    }
                }
            }

            return problems;
        }
    }
}
