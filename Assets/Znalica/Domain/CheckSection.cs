namespace Znalica.Domain
{
    /// <summary>
    /// The four kinds of task on the school-like check's worksheet. Each has its own written instruction; the items of a
    /// section stand together on the page.
    /// </summary>
    public enum CheckSection
    {
        /// <summary>An addition or subtraction with the result unknown: <c>8 + 5 = _</c>.</summary>
        Result,

        /// <summary>An addition or subtraction with an operand unknown: <c>8 + _ = 13</c>.</summary>
        MissingNumber,

        /// <summary>Two numbers with the sign between them to choose: <c>7 _ 12</c>.</summary>
        Sign,

        /// <summary>The number before or after one, as a worksheet strip: <c>_, 14</c> or <c>12, _</c>.</summary>
        Neighbour,
    }
}
