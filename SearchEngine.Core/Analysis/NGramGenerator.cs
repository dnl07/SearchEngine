namespace SearchEngine.Core.Analysis
{
    public static class NGramGenerator
    {
        private const char Edge = '$';

        /// <summary>
        /// Returns the n-grams of the token padded on both sides, so every letter appears in the
        /// same number of n-grams.
        /// </summary>
        public static string[] Padded(string token, int size)
        {
            var edge = new string(Edge, size - 1);
            return Slice($"{edge}{token}{edge}", size);
        }

        /// <summary>
        /// Returns the n-grams of the token padded at the start only, to find tokens that begin
        /// with it.
        /// </summary>
        public static string[] PrefixPadded(string token, int size) =>
            Slice($"{new string(Edge, size - 1)}{token}", size);

        /// <summary>
        /// Returns the n-grams of the token without padding, to find tokens that contain it.
        /// </summary>
        public static string[] Unpadded(string token, int size) => Slice(token, size);

        /// <summary>
        /// Cuts the text into all overlapping substrings of the given size.
        /// </summary>
        private static string[] Slice(string text, int size)
        {
            if (text.Length < size)
                return [];

            var nGrams = new string[text.Length - size + 1];

            for (int i = 0; i < nGrams.Length; i++)
            {
                nGrams[i] = text.Substring(i, size);
            }

            return nGrams;
        }
    }
}
