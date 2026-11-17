namespace Znalica.Domain
{
    /// <summary>How hard an item is. The order is the order items are offered in: easiest first.</summary>
    public enum Difficulty
    {
        /// <summary>Every number in the item is 10 or less.</summary>
        WithinTen,

        /// <summary>Goes beyond 10 without crossing it (<c>12 + 3</c>, <c>17 - 4</c>, <c>10 + 5</c>).</summary>
        BeyondTenWithoutCrossing,

        /// <summary>The fact crosses 10 (<c>8 + 5</c>, <c>13 - 6</c>).</summary>
        CrossingTen,
    }
}
