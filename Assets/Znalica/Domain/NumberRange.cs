namespace Znalica.Domain
{
    /// <summary>The curriculum number range for grade 1: every number in the game lies in 0 to 20.</summary>
    public static class NumberRange
    {
        public const int Min = 0;
        public const int Max = 20;

        public static bool Contains(int number)
        {
            return number >= Min && number <= Max;
        }
    }
}
