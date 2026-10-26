namespace SearchEngine.Core.Documents
{
    public class DocumentStore
    {
        private readonly Dictionary<string, IndexedDocument> _docs = new(StringComparer.Ordinal);
        private readonly long[] _totalFieldLengths;
        private readonly int[] _docCountsPerField;

        public DocumentStore(int fieldCount)
        {
            _totalFieldLengths = new long[fieldCount];
            _docCountsPerField = new int[fieldCount];
        }

        public int Count => _docs.Count;

        public IndexedDocument? Get(string id) => _docs.GetValueOrDefault(id);

        public void Add(IndexedDocument doc)
        {
            _docs[doc.Id] = doc;
            UpdateFieldStatistics(doc, 1);
        }

        public IndexedDocument? Remove(string id)
        {
            if (!_docs.Remove(id, out var doc))
                return null;

            UpdateFieldStatistics(doc, -1);
            return doc;
        }

        public int GetFieldLength(string id, int field) =>
            _docs.TryGetValue(id, out var doc) ? doc.Tokens[field].Length : 0;

        public double GetAverageFieldLength(int field)
        {
            int docCount = _docCountsPerField[field];
            return docCount == 0 ? 0 : (double)_totalFieldLengths[field] / docCount;
        }

        private void UpdateFieldStatistics(IndexedDocument doc, int sign)
        {
            for (int field = 0; field < doc.Tokens.Length; field++)
            {
                int length = doc.Tokens[field].Length;

                if (length == 0)
                    continue;

                _totalFieldLengths[field] += sign * length;
                _docCountsPerField[field] += sign;
            }
        }
    }
}
