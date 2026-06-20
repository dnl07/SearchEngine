namespace SearchEngine.Core.Analysis
{
    public static class Tokenizer
    {
        /// <summary>
        /// Transforms a given text into normalized, by whitespace splitted tokens.
        /// </summary>
        public static string[] Tokenize(string text, HashSet<string>? stopWords = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            var tokens = new List<string>();

            ReadOnlySpan<char> span = text.AsSpan();
            int start = 0;
            for (int i = 0; i <= span.Length; i++)
            {
                if (i == span.Length || char.IsWhiteSpace(span[i]))
                {
                    if (i > start)
                    {
                        var token = ProcessToken(span[start..i], stopWords);
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            tokens.Add(token);
                        }
                    }
                    start = i + 1;
                }
            }

            return tokens.ToArray();
        }

        /// <summary>
        /// Normalizes a token if it is not a stopword.
        /// </summary>
        private static string ProcessToken(
            ReadOnlySpan<char> text,
            HashSet<string>? stopWords = null
        )
        {
            var token = Normalizer.Normalize(text);

            if (string.IsNullOrWhiteSpace(token))
                return "";

            // Remove stopwords
            if (stopWords != null && stopWords.Contains(token))
                return "";

            return token;
        }
    }
}
