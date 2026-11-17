using System.Collections.Generic;
using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// A number slot in a phrase's voice, written <c>{Name}</c>, stands for the clip of a bare numeral
    /// (<c>Number00</c> to <c>Number20</c>). A slot is only for a number that stands alone, never before a noun
    /// (Croatian changes the number's form with the noun's gender).
    /// </summary>
    public static class PhraseSlot
    {
        public static bool TryParse(string voiceItem, out string name)
        {
            name = null;
            if (voiceItem == null || voiceItem.Length < 3 || voiceItem[0] != '{' || voiceItem[voiceItem.Length - 1] != '}')
            {
                return false;
            }

            name = voiceItem.Substring(1, voiceItem.Length - 2);
            return true;
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
