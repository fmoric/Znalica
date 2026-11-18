namespace Znalica.Domain
{
    /// <summary>
    /// What a pad button or a keyboard key means for the answer box. The views map their buttons and keys to this;
    /// <see cref="ArithmeticPlay"/> says what each does. The digits are in order, so <c>Digit0 + n</c> is digit n.
    /// </summary>
    public enum AnswerKey
    {
        Digit0,
        Digit1,
        Digit2,
        Digit3,
        Digit4,
        Digit5,
        Digit6,
        Digit7,
        Digit8,
        Digit9,

        /// <summary>Obriši: removes the last digit.</summary>
        Erase,

        /// <summary>U redu / Enter: gives the answer. After a second wrong answer it does nothing; <see cref="Continue"/> goes on.</summary>
        Confirm,

        /// <summary>Dalje / Enter after a second wrong answer: goes on to the next item. Does nothing at any other time.</summary>
        Continue,
    }
}
