using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using SearchEngine.Core.Exceptions;
using SearchEngine.Models.Indexes;
using SearchEngine.Models.Search;

namespace SearchEngine.Core.Indexes
{
    public sealed partial class IndexRegistry
    {
        private readonly ConcurrentDictionary<string, SearchIndex> _indexes = new(
            StringComparer.Ordinal
        );

        public (SearchIndex Index, IndexChange Change) CreateOrUpdate(
            string name,
            IndexSettings settings
        )
        {
            if (!IndexName().IsMatch(name))
                throw new InvalidInputException(
                    $"'{name}' is not a valid index name. Use 1 to 64 lower-case letters, digits, '-' or '_'."
                );

            var schema = IndexSchema.Create(settings);

            while (true)
            {
                if (_indexes.TryGetValue(name, out var existing))
                {
                    var change = existing.Configure(schema)
                        ? IndexChange.SettingsChanged
                        : IndexChange.Unchanged;
                    return (existing, change);
                }

                var index = new SearchIndex(name, schema);

                if (_indexes.TryAdd(name, index))
                    return (index, IndexChange.Created);
            }
        }

        public SearchIndex Get(string name) =>
            _indexes.TryGetValue(name, out var index)
                ? index
                : throw new IndexNotFoundException(name);

        public bool Delete(string name) => _indexes.TryRemove(name, out _);

        /// <summary>
        /// Runs every query against its index. Fails before searching if any index is missing.
        /// </summary>
        public List<IndexSearchResult> MultiSearch(IReadOnlyList<IndexQuery> queries)
        {
            var indexes = queries.Select(query => Get(query.Index)).ToList();

            return queries
                .Select(
                    (query, i) =>
                        new IndexSearchResult
                        {
                            Index = query.Index,
                            Result = indexes[i].Search(query.Query, query.Limit, query.Explain),
                        }
                )
                .ToList();
        }

        [GeneratedRegex("^[a-z0-9][a-z0-9_-]{0,63}$")]
        private static partial Regex IndexName();
    }
}
