using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// What the child answered. Only a number for now (arithmetic); the comparison sign and the ordering arrive with
    /// those activities.
    /// </summary>
    public readonly struct Answer
    {
        private Answer(int? number)
        {
            Number = number;
        }

        public int? Number { get; }

        public static Answer FromNumber(int number)
        {
            return new Answer(number);
        }

        /// <summary>The form kept in the answer log (<c>13</c>).</summary>
        public string Canonical()
        {
            return Number.HasValue ? Number.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }
    }
}
