namespace Znalica.Domain
{
    /// <summary>A source of random numbers that a test can seed, so that a generated round is repeatable.</summary>
    public interface IRandom
    {
        /// <summary>A number from 0 up to, not including, <paramref name="maxExclusive"/>.</summary>
        int Next(int maxExclusive);
    }
}
