namespace SearchEngine.Core.Analysis
{
    public static class NGramGenerator
    {
        /// <summary>
        /// Generates a list of n-grams for a given token.
        /// </summary>
        public static string[] Generate(
            string token,
            int nGramSize,
            bool prefixEditDistance = false
        )
        {
            if (string.IsNullOrWhiteSpace(token))
                return [];

            // Creates edge symbols used for padding to find proper candidates
            string edgeSymbols = string.Join("", Enumerable.Repeat("$", nGramSize - 1));
            var paddedToken = prefixEditDistance
                ? $"{edgeSymbols}{token}"
                : $"{edgeSymbols}{token}{edgeSymbols}";

            List<string> nGrams = new List<string>(paddedToken.Length - nGramSize + 1);

            ReadOnlySpan<char> span = paddedToken.AsSpan();

            // "nGram" -> ["$n", ""nG", "Gr", "ra", "am", "m$"] with size = 2
            for (int i = 0; i < span.Length - nGramSize + 1; i++)
            {
                nGrams.Add(span.Slice(i, nGramSize).ToString());
            }

            return nGrams.ToArray();
        }
    }
}
