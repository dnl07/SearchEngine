using System.Diagnostics;
using System.Text.Json;
using SearchEngine.Core.Documents;
using SearchEngine.Core.Fuzzy;
using SearchEngine.Core.Indexing;
using SearchEngine.Core.Options;
using SearchEngine.Core.Ranking;

namespace SearchEngine.Tests.CoreTests
{
    public static class TestHelper
    {
        public static async Task<SearchDocument[]> GetDocumentsFromJson(string filename)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            var json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<SearchDocument[]>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return data ?? [];
        }

        public static TestContext CreateFromDocs(SearchDocument[] docs)
        {
            // Options
            var indexOptions = new IndexOptions { NGramSize = 3 };
            var queryOptions = new QueryOptions { MaxEditDistance = 2 };

            // Registries
            var tokenRegistry = new TokenRegistry();
            var docStore = new DocumentStore();

            // Indexes
            var invertedIndex = new InvertedIndex();
            var nGramIndex = new NGramIndex(indexOptions.NGramSize);

            // Ranking
            var scoringEngine = new ScoringEngine();

            // Fuzzy
            var matcher = new FuzzyMatcher(nGramIndex, indexOptions);

            foreach (var doc in docs)
            {
                doc.Id = Guid.NewGuid();

                doc.Tokenize();

                docStore.Add(doc);
                invertedIndex.AddDocument(doc);

                var allTokens = doc.AllTokens;

                foreach (string token in allTokens)
                {
                    var id = tokenRegistry.Add(token);

                    if (id != -1)
                    {
                        nGramIndex.AddToken(token, id);
                    }
                }
            }

            return new TestContext
            {
                TokenRegistry = tokenRegistry,
                NGramIndex = nGramIndex,
                InvertedIndex = invertedIndex,
                DocumentStore = docStore,
                IndexOptions = indexOptions,
                QueryOptions = queryOptions,
                FuzzyMatcher = matcher,
                ScoringEngine = scoringEngine,
            };
        }
    }

    public class TestContext
    {
        public TokenRegistry TokenRegistry { get; init; } = null!;
        public NGramIndex NGramIndex { get; init; } = null!;
        public InvertedIndex InvertedIndex { get; init; } = null!;
        public DocumentStore DocumentStore { get; init; } = null!;
        public IndexOptions IndexOptions { get; init; } = null!;
        public QueryOptions QueryOptions { get; init; } = null!;
        public FuzzyMatcher FuzzyMatcher { get; init; } = null!;
        public ScoringEngine ScoringEngine { get; init; } = null!;
    }
}
