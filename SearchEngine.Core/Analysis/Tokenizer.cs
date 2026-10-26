namespace SearchEngine.Core.Analysis
{
    public static class Tokenizer
    {
        /// <summary>
        /// Splits the text into the tokens to index: every word joined, plus its parts if it has
        /// several, without stopwords.
        /// </summary>
        public static List<string> TokenizeForIndex(string? text, IReadOnlySet<string> stopwords)
        {
            var tokens = new List<string>();

            foreach (var parts in Words(text))
            {
                if (parts.Count > 1)
                {
                    tokens.AddRange(parts.Where(part => !stopwords.Contains(part)));
                }

                var joined = string.Concat(parts);

                if (!stopwords.Contains(joined))
                {
                    tokens.Add(joined);
                }
            }

            return tokens;
        }

        /// <summary>
        /// Splits the query into distinct terms, joining the parts of each word and dropping
        /// stopwords.
        /// </summary>
        public static List<string> TokenizeQuery(string? text, IReadOnlySet<string> stopwords) =>
            Words(text)
                .Select(parts => string.Concat(parts))
                .Where(term => !stopwords.Contains(term))
                .Distinct()
                .ToList();

        /// <summary>
        /// Splits the text at separators and returns the normalized parts of each word.
        /// </summary>
        private static IEnumerable<List<string>> Words(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            int start = 0;

            for (int i = 0; i <= text.Length; i++)
            {
                if (i < text.Length && !IsSeparator(text[i]))
                    continue;

                if (i > start)
                {
                    var parts = Normalizer.SplitParts(text[start..i]);

                    if (parts.Count > 0)
                    {
                        yield return parts;
                    }
                }

                start = i + 1;
            }
        }

        /// <summary>
        /// Returns whether the character separates two words.
        /// </summary>
        private static bool IsSeparator(char c) =>
            char.IsWhiteSpace(c)
            || c
                is ','
                    or ';'
                    or ':'
                    or '!'
                    or '?'
                    or '('
                    or ')'
                    or '['
                    or ']'
                    or '{'
                    or '}'
                    or '"'
                    or '|';
    }
}
