namespace SearchEngine.Core.Analysis
{
    public static class Tokenizer
    {
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

        public static List<string> TokenizeQuery(string? text, IReadOnlySet<string> stopwords) =>
            Words(text)
                .Select(parts => string.Concat(parts))
                .Where(term => !stopwords.Contains(term))
                .Distinct()
                .ToList();

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
