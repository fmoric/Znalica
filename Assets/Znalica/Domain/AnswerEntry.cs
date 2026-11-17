namespace Znalica.Domain
{
    /// <summary>
    /// The answer box's rule: a keystroke that would make the value pass 20 is refused, so no answer outside 0-20 can
    /// be entered (after <c>2</c> only <c>0</c> is accepted). The pad and the keys only call this.
    /// </summary>
    public sealed class AnswerEntry
    {
        private int? _value;

        /// <summary>The number typed so far, or null while the box is empty.</summary>
        public int? Value => _value;

        /// <summary>True if the digit was taken; false if it is not a digit or would make the value pass 20.</summary>
        public bool TryType(int digit)
        {
            if (digit < 0 || digit > 9)
            {
                return false;
            }

            int next = (_value ?? 0) * 10 + digit;
            if (next > NumberRange.Max)
            {
                return false;
            }

            _value = next;
            return true;
        }

        /// <summary>Removes the last digit; an empty box stays empty.</summary>
        public void Erase()
        {
            _value = _value.HasValue && _value.Value >= 10 ? _value.Value / 10 : (int?)null;
        }
    }
}
