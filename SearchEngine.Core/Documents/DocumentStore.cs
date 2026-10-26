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

        /// <summary>
        /// Returns the document with the given id, or null.
        /// </summary>
        public IndexedDocument? Get(string id) => _docs.GetValueOrDefault(id);

        /// <summary>
        /// Stores the document and adds its field lengths to the statistics.
        /// </summary>
        public void Add(IndexedDocument doc)
        {
            _docs[doc.Id] = doc;
            UpdateFieldStatistics(doc, 1);
        }

        /// <summary>
        /// Removes the document and its field lengths from the statistics and returns it, or null
        /// if it does not exist.
        /// </summary>
        public IndexedDocument? Remove(string id)
        {
            if (!_docs.Remove(id, out var doc))
                return null;

            UpdateFieldStatistics(doc, -1);
            return doc;
        }

        /// <summary>
        /// Returns the number of tokens in a field of the document.
        /// </summary>
        public int GetFieldLength(string id, int field) =>
            _docs.TryGetValue(id, out var doc) ? doc.Tokens[field].Length : 0;

        /// <summary>
        /// Returns the average number of tokens in a field over all documents that have the field.
        /// </summary>
        public double GetAverageFieldLength(int field)
        {
            int docCount = _docCountsPerField[field];
            return docCount == 0 ? 0 : (double)_totalFieldLengths[field] / docCount;
        }

        /// <summary>
        /// Adds the field lengths of the document to the statistics, or subtracts them for a
        /// negative sign.
        /// </summary>
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
