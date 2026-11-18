using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>A multiplication of two factors, asked in words, not digits, and not spoken.</summary>
    public sealed class GateChallenge
    {
        public GateChallenge(int first, int second)
        {
            First = first;
            Second = second;
        }

        public int First { get; }

        public int Second { get; }

        public int Answer => First * Second;

        /// <summary>The question as text, each factor as its number word (<c>Koliko je šest puta sedam?</c>).</summary>
        public string Text(PhraseCatalog catalog)
        {
            var args = new Dictionary<string, string>
            {
                { "First", catalog.Text(NumberWordKey(First)) },
                { "Second", catalog.Text(NumberWordKey(Second)) },
            };
            return catalog.Text(PhraseKey.GateQuestion, args);
        }

        private static PhraseKey NumberWordKey(int factor)
        {
            return PhraseKey.NumberWord3 + (factor - ParentGate.FactorMin);
        }
    }
}
