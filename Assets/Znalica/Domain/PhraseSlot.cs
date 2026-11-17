using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Znalica.Domain
{
    /// <summary>
    /// A number slot in a phrase's voice, written <c>{Name}</c>, stands for the clip of a bare numeral
    /// (<c>Number00</c> to <c>Number20</c>). A slot is only for a number that stands alone, never before a noun
    /// (Croatian changes the number's form with the noun's gender).
    /// </summary>
    public static class PhraseSlot
    {
        private const string NamePattern = "[A-Za-z0-9_]+";

        /// <summary>A slot inside a text; group 1 is its name. The one definition of what a slot name may hold.</summary>
        public static readonly Regex InText = new Regex(@"\{(" + NamePattern + @")\}", RegexOptions.Compiled);

        private static readonly Regex WholeItem = new Regex(@"^\{(" + NamePattern + @")\}\z", RegexOptions.Compiled);

        public static bool TryParse(string voiceItem, out string name)
        {
            Match match = voiceItem == null ? Match.Empty : WholeItem.Match(voiceItem);
            name = match.Success ? match.Groups[1].Value : null;
            return match.Success;
        }

        public static string NumberClipName(int number)
        {
            return "Number" + number.ToString("00", CultureInfo.InvariantCulture);
        }

        public static IEnumerable<string> AllNumberClipNames()
        {
            for (int number = NumberRange.Min; number <= NumberRange.Max; number++)
            {
                yield return NumberClipName(number);
            }
        }
    }
}
