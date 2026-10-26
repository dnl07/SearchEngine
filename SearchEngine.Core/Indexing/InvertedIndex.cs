using SearchEngine.Core.Documents;

namespace SearchEngine.Core.Indexing
{
    public class InvertedIndex
    {
        private readonly Dictionary<string, TokenPosting> _postings = new(StringComparer.Ordinal);
        private readonly int _fieldCount;

        public InvertedIndex(int fieldCount)
        {
            _fieldCount = fieldCount;
        }

        public int Count => _postings.Count;

        public bool Contains(string token) => _postings.ContainsKey(token);

        public TokenPosting? Get(string token) => _postings.GetValueOrDefault(token);

        /// <returns>The tokens that were not part of the index before.</returns>
        public List<string> AddDocument(IndexedDocument doc)
        {
            var newTokens = new List<string>();

            for (int field = 0; field < _fieldCount; field++)
            {
                foreach (var group in doc.Tokens[field].GroupBy(token => token))
                {
                    if (!_postings.TryGetValue(group.Key, out var posting))
                    {
                        posting = new TokenPosting(_fieldCount);
                        _postings[group.Key] = posting;
                        newTokens.Add(group.Key);
                    }

                    var docs = posting.Fields[field] ??= new(StringComparer.Ordinal);
                    docs[doc.Id] = group.Count();
                }
            }

            foreach (var token in doc.Tokens.SelectMany(tokens => tokens).Distinct())
            {
                _postings[token].DocumentCount++;
            }

            return newTokens;
        }

        /// <returns>The tokens that are no longer part of the index.</returns>
        public List<string> RemoveDocument(IndexedDocument doc)
        {
            var removedTokens = new List<string>();

            foreach (var token in doc.Tokens.SelectMany(tokens => tokens).Distinct())
            {
                if (_postings.TryGetValue(token, out var posting))
                {
                    posting.DocumentCount--;
                }
            }

            for (int field = 0; field < _fieldCount; field++)
            {
                foreach (var token in doc.Tokens[field].Distinct())
                {
                    if (!_postings.TryGetValue(token, out var posting))
                        continue;

                    var docs = posting.Fields[field];

                    if (docs is null || !docs.Remove(doc.Id) || docs.Count > 0)
                        continue;

                    posting.Fields[field] = null;

                    if (posting.IsEmpty)
                    {
                        _postings.Remove(token);
                        removedTokens.Add(token);
                    }
                }
            }

            return removedTokens;
        }
    }
}
