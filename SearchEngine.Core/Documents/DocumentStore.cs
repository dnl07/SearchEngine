using SearchEngine.Models.Indexing;

namespace SearchEngine.Core.Documents
{
    public class DocumentStore
    {
        private readonly Dictionary<Guid, SearchDocument> _docs = new();
        private readonly Dictionary<Field, long> _totalFieldLengths = new();
        private readonly Dictionary<Field, int> _docCountsPerField = new();

        public int Count => _docs.Count;

        public DocumentStore()
        {
            foreach (Field field in Enum.GetValues(typeof(Field)))
            {
                _totalFieldLengths[field] = 0;
                _docCountsPerField[field] = 0;
            }
        }

        public SearchDocument? Get(Guid id)
        {
            if (_docs.TryGetValue(id, out var doc))
            {
                return doc;
            }
            return null;
        }

        /// <summary>
        /// Adds a document and updates field statistics.
        /// </summary>
        public void Add(SearchDocument doc)
        {
            if (_docs.ContainsKey(doc.Id))
                return;

            _docs[doc.Id] = doc;
            UpdateAllFields(doc);
        }

        public void Remove(Guid id)
        {
            var doc = _docs[id];

            RemoveFieldLengths(doc.TitleTokens, Field.Title);
            RemoveFieldLengths(doc.DescriptionTokens, Field.Description);
            RemoveFieldLengths(doc.TagsTokens, Field.Tags);

            _docs.Remove(id);
        }

        private void UpdateAllFields(SearchDocument doc)
        {
            void UpdateFieldLengths(string[] tokens, Field field)
            {
                if (tokens.Length == 0)
                    return;

                _totalFieldLengths[field] += tokens.Length;
                _docCountsPerField[field]++;
            }

            UpdateFieldLengths(doc.TitleTokens, Field.Title);
            UpdateFieldLengths(doc.DescriptionTokens, Field.Description);
            UpdateFieldLengths(doc.TagsTokens, Field.Tags);
        }

        private void RemoveFieldLengths(string[] tokens, Field field)
        {
            if (tokens.Length == 0)
                return;

            _totalFieldLengths[field] -= tokens.Length;
            _docCountsPerField[field]--;
        }

        public int GetFieldLength(Guid id, Field field)
        {
            if (!_docs.TryGetValue(id, out var doc))
                return 0;

            return doc.GetFieldTokens(field).Length;
        }

        /// <summary>
        /// Returns the average field length in tokens.
        /// </summary>
        public double GetAverageFieldLength(Field field)
        {
            int docCount = _docCountsPerField[field];
            if (docCount == 0)
                return 0;

            return (double)_totalFieldLengths[field] / docCount;
        }

        public int GetCount(Field field)
        {
            return _docCountsPerField[field];
        }
    }
}
