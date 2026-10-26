using SearchEngine.Core.Analysis;

namespace SearchEngine.Core.Indexing
{
    public class NGramIndex
    {
        public const int Size = 3;

        private static readonly HashSet<string> NoTokens = new();

        private readonly Dictionary<string, HashSet<string>> _tokensByNGram = new(
            StringComparer.Ordinal
        );

        public int Count => _tokensByNGram.Count;

        public void AddToken(string token)
        {
            foreach (var nGram in NGramGenerator.Padded(token, Size).Distinct())
            {
                if (!_tokensByNGram.TryGetValue(nGram, out var tokens))
                {
                    tokens = new HashSet<string>(StringComparer.Ordinal);
                    _tokensByNGram[nGram] = tokens;
                }

                tokens.Add(token);
            }
        }

        public void RemoveToken(string token)
        {
            foreach (var nGram in NGramGenerator.Padded(token, Size).Distinct())
            {
                if (_tokensByNGram.TryGetValue(nGram, out var tokens))
                {
                    tokens.Remove(token);

                    if (tokens.Count == 0)
                    {
                        _tokensByNGram.Remove(nGram);
                    }
                }
            }
        }

        public IReadOnlySet<string> GetTokens(string nGram) =>
            _tokensByNGram.TryGetValue(nGram, out var tokens) ? tokens : NoTokens;
    }
}
