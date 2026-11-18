using System;
using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// The identity of an item in the answer log, the schedule and the progress view: a canonical string such as
    /// <c>A:8+5=?</c>. <see cref="ToString"/> and <see cref="TryParse"/> round-trip. Whether a key names an item that
    /// can exist is the item set's question (<see cref="IItemSet.TryFind"/>), not this type's.
    /// </summary>
    public readonly struct ItemKey : IEquatable<ItemKey>
    {
        private const int MaxLength = 64;

        private readonly string _value;

        private ItemKey(string value)
        {
            _value = value;
        }

        /// <summary>True for a non-empty text of printable ASCII without spaces; that is the form every key has.</summary>
        public static bool TryParse(string text, out ItemKey key)
        {
            key = default;
            if (string.IsNullOrEmpty(text) || text.Length > MaxLength)
            {
                return false;
            }

            foreach (char c in text)
            {
                if (c < '!' || c > '~')
                {
                    return false;
                }
            }

            key = new ItemKey(text);
            return true;
        }

        /// <summary>For the item classes, which build only well-formed keys.</summary>
        internal static ItemKey Of(string text)
        {
            if (!TryParse(text, out ItemKey key))
            {
                throw new ArgumentException("Not a well-formed item key: " + text, nameof(text));
            }

            return key;
        }

        /// <summary>
        /// A number as a key writes it: digits only, no leading zero, within 0-20. The item classes read the numbers of a
        /// key with this, so a key written another way (<c>07</c>, <c>+7</c>, <c>21</c>) names no item.
        /// </summary>
        internal static bool TryReadNumber(string text, out int number)
        {
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out number)
                && number.ToString(CultureInfo.InvariantCulture) == text
                && NumberRange.Contains(number);
        }

        public bool Equals(ItemKey other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is ItemKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}
