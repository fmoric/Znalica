namespace Znalica.Domain
{
    public enum RoundState
    {
        /// <summary>An item is asked and waits for <see cref="RoundSession.Submit"/>.</summary>
        AwaitingAnswer,

        /// <summary>
        /// A wrong answer's explanation is shown. After the first wrong answer to an item, <see cref="RoundSession.Submit"/>
        /// asks it again; after the second, only <see cref="RoundSession.Continue"/> goes on.
        /// </summary>
        ShowingExplanation,

        /// <summary>No items are left.</summary>
        Ended,
    }
}
