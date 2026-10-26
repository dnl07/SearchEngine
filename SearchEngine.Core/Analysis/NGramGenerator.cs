namespace SearchEngine.Core.Analysis
{
    public static class NGramGenerator
    {
        private const char Edge = '$';

        /// <summary>
        /// N-grams of the token padded on both sides: "jeans" -> ["$$j", "$je", "jea", ..., "s$$"].
        /// </summary>
        public static string[] Padded(string token, int size)
        {
            var edge = new string(Edge, size - 1);
            return Slice($"{edge}{token}{edge}", size);
        }

        /// <summary>
        /// N-grams of the token padded at the start only, used to find tokens with the token as prefix.
        /// </summary>
        public static string[] PrefixPadded(string token, int size) =>
            Slice($"{new string(Edge, size - 1)}{token}", size);

        public static string[] Unpadded(string token, int size) => Slice(token, size);

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
