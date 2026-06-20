namespace SearchEngine.Core.Fuzzy
{
    public static class EditDistance
    {
        /// <summary>
        /// Initiates the recursive calculation and returns the edit distance of two tokens.
        /// </summary>
        /// <param name="t1">First token.</param>
        /// <param name="t2">Second token.</param>
        public static int Calculate(string t1, string t2)
        {
            return CalculateRecursive(t1, t2, t1.Length, t2.Length);
        }

        /// <summary>
        /// Recursive function to calculate the number of operations needed to convert the first token to the second token.
        /// </summary>
        /// <param name="t1">First token.</param>
        /// <param name="t2">Second token.</param>
        /// <param name="m">Length of the first token.</param>
        /// <param name="n">Length of the second token.</param>
        private static int CalculateRecursive(string t1, string t2, int m, int n)
        {
            if (m == 0)
                return n;

            if (n == 0)
                return m;

            // if the last letter of both tokens matches, no operation is needed
            if (t1[m - 1] == t2[n - 1])
            {
                return CalculateRecursive(t1, t2, m - 1, n - 1);
            }

            return 1
                + Math.Min(
                    Math.Min(
                        CalculateRecursive(t1, t2, m, n - 1), // insertion
                        CalculateRecursive(t1, t2, m - 1, n)
                    ), // deletion
                    CalculateRecursive(t1, t2, m - 1, n - 1)
                ); // replacement
        }
    }
}
