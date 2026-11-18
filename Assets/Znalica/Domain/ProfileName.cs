namespace Znalica.Domain
{
    /// <summary>
    /// The rule for a profile's display name: 1 to 20 characters after trimming, only letters, spaces and hyphens, so
    /// that no surname, birth date or other identifying detail has a field to go in (functional spec, data list).
    /// </summary>
    public static class ProfileName
    {
        public const int MaxLength = 20;

        /// <summary>True, with the trimmed name, when the input is acceptable; otherwise false and an empty name.</summary>
        public static bool TryNormalize(string input, out string name)
        {
            name = string.Empty;
            if (input == null)
            {
                return false;
            }

            string trimmed = input.Trim();
            if (trimmed.Length < 1 || trimmed.Length > MaxLength)
            {
                return false;
            }

            foreach (char c in trimmed)
            {
                if (!char.IsLetter(c) && c != ' ' && c != '-')
                {
                    return false;
                }
            }

            name = trimmed;
            return true;
        }
    }
}
