using SearchEngine.Core.Search;
using SearchEngine.Models.Documents;
using SearchEngine.Models.Indexes;
using SearchEngine.Models.Search;

namespace SearchEngine.Core.Indexes
{
    /// <summary>
    /// A named index. Writes take the write lock, searches the read lock.
    /// </summary>
    public sealed class SearchIndex
    {
        private readonly ReaderWriterLockSlim _lock = new();
        private IndexSchema _schema;
        private Engine _engine;

        public SearchIndex(string name, IndexSchema schema)
        {
            Name = name;
            _schema = schema;
            _engine = new Engine(schema);
        }

        public string Name { get; }

        public IndexSettings Settings => Read(() => _schema.Settings);

        public int DocumentCount => Read(() => _engine.Count);

        /// <returns>True if the settings changed, which empties the index.</returns>
        public bool Configure(IndexSchema schema) =>
            Write(() =>
            {
                if (_schema.IsEquivalentTo(schema))
                    return false;

                _schema = schema;
                _engine = new Engine(schema);
                return true;
            });

        /// <summary>
        /// Replaces all documents. The new engine is built outside the lock, so searches keep
        /// running on the old documents until the swap.
        /// </summary>
        public int Replace(IReadOnlyCollection<SearchDocument> docs)
        {
            var schema = Read(() => _schema);
            var engine = Build(schema, docs);

            return Write(() =>
            {
                if (!ReferenceEquals(schema, _schema))
                {
                    engine = Build(_schema, docs);
                }

                _engine = engine;
                return _engine.Count;
            });
        }

        public int Upsert(IReadOnlyCollection<SearchDocument> docs) =>
            Write(() =>
            {
                var analyzed = docs.Select(_schema.Analyze).ToList();

                foreach (var doc in analyzed)
                {
                    _engine.Upsert(doc);
                }

                return _engine.Count;
            });

        public bool Remove(string id) => Write(() => _engine.Remove(id));

        public SearchResult Search(string? query, int? limit, bool explain) =>
            Read(() => _engine.Search(query, limit, explain));

        private static Engine Build(IndexSchema schema, IEnumerable<SearchDocument> docs)
        {
            var engine = new Engine(schema);

            foreach (var doc in docs)
            {
                engine.Upsert(schema.Analyze(doc));
            }

            return engine;
        }

        private T Read<T>(Func<T> action)
        {
            _lock.EnterReadLock();
            try
            {
                return action();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private T Write<T>(Func<T> action)
        {
            _lock.EnterWriteLock();
            try
            {
                return action();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
