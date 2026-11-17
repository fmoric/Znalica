namespace Znalica.Domain
{
    /// <summary>One piece of an item as shown on screen: a number, a sign, or the box the child fills in.</summary>
    public readonly struct ItemToken
    {
        public ItemToken(string text, bool isUnknown)
        {
            Text = text;
            IsUnknown = isUnknown;
        }

        /// <summary>What to draw; for the unknown, the <see cref="ArithmeticItem.UnknownMark"/> of the item key (the screen draws a box instead).</summary>
        public string Text { get; }

        /// <summary>True for the number the child has to find.</summary>
        public bool IsUnknown { get; }
    }
}
