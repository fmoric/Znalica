using System;

namespace Znalica.Domain
{
    /// <summary>
    /// One catalog entry: the text shown on screen, and the clips spoken for it, in order. A voice item is a clip
    /// name (<c>PraiseRight</c>) or a number slot (<c>{Start}</c>, see <see cref="PhraseSlot"/>). An empty voice
    /// means the phrase is screen-only.
    /// </summary>
    public sealed class Phrase
    {
        public Phrase(string text, string[] voice)
        {
            Text = text ?? string.Empty;
            Voice = voice ?? Array.Empty<string>();
        }

        public string Text { get; }

        public string[] Voice { get; }
    }
}
