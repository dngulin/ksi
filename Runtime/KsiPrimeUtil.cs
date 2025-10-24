namespace Ksi
{
    /// <summary>
    /// Utility class to work with prime numbers.
    /// Used by generated hash table implementations produced by <see cref="KsiHashTableAttribute"/>.
    /// </summary>
    public static class KsiPrimeUtil
    {
        /// <summary>
        /// Checks if the given number is a prime number.
        /// </summary>
        /// <param name="n">number to check</param>
        /// <returns><c>true</c> if the given number is a prime number, or <c>false</c> otherwise</returns>
        public static bool IsPrime(int n)
        {
            if (n <= 1)
                return false;

            if (n <= 3)
                return true;

            if (n % 2 == 0 || n % 3 == 0)
                return false;

            for (var i = 5; i * i <= n; i += 6)
                if (n % i == 0 || n % (i + 2) == 0)
                    return false;

            return true;
        }

        /// <summary>
        /// Finds the smallest prime number greater than a given number.
        /// </summary>
        /// <param name="n">number to start the search</param>
        /// <returns>The smallest prime number bigger than a given number</returns>
        public static int NextPrime(int n)
        {
            if (n < 2)
                return 2;

            if (n % 2 == 0)
                n--;

            while (true)
            {
                n += 2;
                if (IsPrime(n))
                    return n;
            }
        }

        /// <summary>
        /// Finds the smallest prime number equal or greater than a given number.
        /// </summary>
        /// <param name="n">number to start the search</param>
        /// <returns>The smallest prime number equal or greater than a given number</returns>
        public static int EqualOrNextPrime(int n) => IsPrime(n) ? n : NextPrime(n);
    }
}