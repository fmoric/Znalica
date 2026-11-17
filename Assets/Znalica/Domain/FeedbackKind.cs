namespace Znalica.Domain
{
    public enum FeedbackKind
    {
        /// <summary>A right answer: brief praise. It sits beside the next item and holds nothing up.</summary>
        Praise,

        /// <summary>A wrong answer: the explanation, shown and spoken in place.</summary>
        Explanation,
    }
}
