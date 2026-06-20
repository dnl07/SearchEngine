using SearchEngine.Core.Analysis;

namespace SearchEngine.Core.Indexing
{
    public class NGramIndex
    {
        private readonly Dictionary<string, HashSet<int>> _nGramToToken = new();
        private readonly Dictionary<int, string[]> _tokenToNGram = new();

        private int _nGramSize { get; }

        public int Count => _nGramToToken.Count;

        public NGramIndex(int nGramSize)
        {
            _nGramSize = nGramSize;
        }

        public void AddToken(string token, int id)
        {
            var nGrams = NGramGenerator.Generate(token, _nGramSize);

            _tokenToNGram.TryAdd(id, nGrams.ToArray());

            foreach (var nGram in nGrams)
            {
                if (_nGramToToken.TryGetValue(nGram, out var tokenIds))
                {
                    tokenIds.Add(id);
                }
                else
                {
                    _nGramToToken[nGram] = [id];
                }
            }
        }

        public void RemoveToken(string token, int id)
        {
            var nGrams = NGramGenerator.Generate(token, _nGramSize);

            _tokenToNGram.Remove(id);

            foreach (var nGram in nGrams)
            {
                if (_nGramToToken.TryGetValue(nGram, out var ids))
                {
                    ids.Remove(id);
                    if (ids.Count == 0)
                        _nGramToToken.Remove(nGram);
                }
            }
        }

        public HashSet<int> GetCandidates(string nGram)
        {
            if (_nGramToToken.TryGetValue(nGram, out var candidates))
            {
                return candidates ?? [];
            }
            return [];
        }

        public string[] GetNGrams(int tokenId)
        {
            if (_tokenToNGram.TryGetValue(tokenId, out var nGrams))
            {
                return nGrams;
            }
            return [];
        }
    }
}
