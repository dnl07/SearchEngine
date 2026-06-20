using SearchEngine.Core.Documents;
using SearchEngine.Models.Indexing;

namespace SearchEngine.Core.Indexing
{
    public class InvertedIndex
    {
        private readonly Dictionary<string, TokenPosting> _invertedIndex = new();

        public int Count => _invertedIndex.Count;

        public void AddDocument(SearchDocument doc)
        {
            AddTokens(doc, Field.Title);
            AddTokens(doc, Field.Description);
            AddTokens(doc, Field.Tags);
        }

        public void RemoveDocument(SearchDocument doc)
        {
            RemoveTokens(doc, Field.Title);
            RemoveTokens(doc, Field.Description);
            RemoveTokens(doc, Field.Tags);
        }

        private void AddTokens(SearchDocument doc, Field field)
        {
            var localCounts = new Dictionary<string, int>();

            foreach (string token in doc.GetFieldTokens(field))
            {
                if (!localCounts.TryGetValue(token, out var count))
                {
                    localCounts[token] = 1;
                }
                else
                {
                    localCounts[token] = count + 1;
                }
            }

            short fieldOrdinal = FieldOrdinal.FieldToOrdinal(field);

            foreach (var (token, freq) in localCounts)
            {
                if (!_invertedIndex.TryGetValue(token, out var posting))
                {
                    posting = new TokenPosting();
                    _invertedIndex[token] = posting;
                }

                posting.DocIds.Add(doc.Id);
                posting.Fields.Add(fieldOrdinal);
                posting.Frequencies.Add(freq);
            }
        }

        private void RemoveTokens(SearchDocument doc, Field field)
        {
            var tokens = doc.GetFieldTokens(field);

            short f = FieldOrdinal.FieldToOrdinal(field);

            foreach (var token in tokens)
            {
                if (!_invertedIndex.TryGetValue(token, out var posting))
                {
                    continue;
                }

                var docs = posting.DocIds;
                var fields = posting.Fields;
                var freqs = posting.Frequencies;

                for (int i = docs.Count - 1; i >= 0; i--)
                {
                    if (docs[i] == doc.Id && fields[i] == f)
                    {
                        var last = docs.Count - 1;

                        docs[i] = docs[last];
                        fields[i] = fields[last];
                        freqs[i] = freqs[last];

                        docs.RemoveAt(last);
                        fields.RemoveAt(last);
                        freqs.RemoveAt(last);
                    }
                }

                if (posting.DocIds.Count == 0)
                {
                    _invertedIndex.Remove(token);
                }
            }
        }

        public TokenPosting? GetTokenPosting(string token)
        {
            if (_invertedIndex.TryGetValue(token, out var posting))
            {
                return posting;
            }
            return null;
        }
    }
}
