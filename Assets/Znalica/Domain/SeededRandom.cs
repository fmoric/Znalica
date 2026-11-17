using System;

namespace Znalica.Domain
{
    /// <summary>An <see cref="IRandom"/> over <see cref="System.Random"/>; the same seed gives the same sequence.</summary>
    public sealed class SeededRandom : IRandom
    {
        private readonly Random _random;

        public SeededRandom(int seed)
        {
            _random = new Random(seed);
        }

        public int Next(int maxExclusive)
        {
            return _random.Next(maxExclusive);
        }
    }
}
