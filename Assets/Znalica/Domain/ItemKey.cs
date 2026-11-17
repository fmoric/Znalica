using System;

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
