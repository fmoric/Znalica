namespace Znalica.Domain
{
    /// <summary>
    /// First: the first time the item is asked this round. Retry: straight after an explanation. Return: asked again
    /// later in the same round. Right first time means <c>Attempt == First</c> and a right answer.
    /// </summary>
    public enum AttemptKind
    {
        First,
        Retry,
        Return,
    }
}
