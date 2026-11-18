namespace Znalica.Domain
{
    /// <summary>What a key press at the parent gate did.</summary>
    public enum GateResult
    {
        /// <summary>The key only changed the answer box, or there was nothing to confirm.</summary>
        Typing,

        /// <summary>The right answer: the gate is open.</summary>
        Passed,

        /// <summary>A wrong answer: a new question is shown.</summary>
        Wrong,

        /// <summary>
        /// The gate is waiting: either this wrong answer was the third in a row, or the press came during the wait and
        /// was refused. While it waits the gate takes no answer.
        /// </summary>
        Locked,
    }
}
