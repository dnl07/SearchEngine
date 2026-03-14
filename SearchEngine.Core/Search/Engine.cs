using SearchEngine.Core.Analysis;
using SearchEngine.Core.Documents;
using SearchEngine.Core.Fuzzy;
using SearchEngine.Core.Indexing;
using SearchEngine.Core.Options;
using SearchEngine.Core.Ranking;
using SearchEngine.Models.Scoring;
using SearchEngine.Models.Search;
using System.Diagnostics.CodeAnalysis;

namespace SearchEngine.Core.Search {
    public class Engine {
        private IndexOptions _options = new();
        private bool _initialized = false;

        private readonly DocumentStore _docs = new();
        private readonly TokenRegistry _tokens = new();

        private readonly InvertedIndex _invertedIndex = new();
        private NGramIndex _nGramIndex = null!;

        private FuzzyMatcher _fuzzyMatcher = null!;
        private QueryExpander _expander = null!;
        private ScoringEngine _scoring = null!;

        private bool _isRunning = false;
        private DateTime _startedAt;

        private HashSet<string> _stopwords = new();

        public Engine() {
            _isRunning = true;
            _startedAt = DateTime.Now;
        }

        public void Initialize(IndexOptions options) {
            if (_initialized) {
                throw new InvalidOperationException("SearchEngine is already initialized.");
            }

            _options = options;
            _nGramIndex = new NGramIndex(_options.NGramSize);
            _fuzzyMatcher = new FuzzyMatcher(_nGramIndex, _options);
            _expander = new QueryExpander();
            _scoring = new ScoringEngine();

            _initialized = true;
            _stopwords = StopwordsProvider.ResolveStopwords(_options);
        }

        private void EnsureInitialized() {
            if (!_initialized) {
                throw new InvalidOperationException("SearchEngine is not initialized.");
            }
        }

        public SearchDocument AddDocument(SearchDocument doc) {
            if (!_initialized) {
                Initialize(new IndexOptions());
                _initialized = true;
            }

            if (_options.UseOwnIds) {
                if (doc.Id == Guid.Empty) {
                    throw new ArgumentException("Document must have an Id when UseOwnIds is true.");
                }

                if (_docs.Get(doc.Id) is not null) {
                    throw new ArgumentException($"Document with id {doc.Id} already exists.");
                }
            }

            if (!_options.UseOwnIds) {
                doc.Id = Guid.NewGuid();
            }

            doc.Tokenize(_stopwords);

            _docs.Add(doc);
            _invertedIndex.AddDocument(doc);

            var allTokens = doc.AllTokens;

            foreach (string token in allTokens) {
                var id = _tokens.Add(token);
    
                if (id != -1) {
                    _nGramIndex.AddToken(token, id);
                }
            }

            return doc;
        }

        public void RemoveDocument(Guid docId) {
            var doc = _docs.Get(docId);

            if (doc == null) return;

            _docs.Remove(doc.Id);
            _invertedIndex.RemoveDocument(doc);
            
            foreach (var token in doc.AllTokens) {
                var tokenId = _tokens.GetIdOfToken(token);
                if (tokenId == -1) continue;

                if (_invertedIndex.GetTokenPosting(token) != null) continue;

                _tokens.Remove(token);
                _nGramIndex.RemoveToken(token, tokenId);
            }
        }

        public void UpdateDocument(Guid oldId, SearchDocument newDoc) {
            RemoveDocument(oldId);
            AddDocument(newDoc);
        }

        public SearchResult Search(string request, bool explain, QueryOptions? options = null) {
            EnsureInitialized();
        
            options ??= new QueryOptions();

            var result = new SearchResult();
            result.Query = request;

            // fuzzy string matching
            var expanded = _expander.Expand(request, _fuzzyMatcher, _tokens, _stopwords, options);

            if (explain) {
                result.MatchedTokens = expanded.Select(e => e.token).ToList();
            }

            // creates scores
            Dictionary<Guid, ScoreResult> scores = _scoring.ScoreDocuments(expanded, _invertedIndex, _docs, options, explain);

            // uses a min-heap to improve search performance
            var topK = new PriorityQueue<KeyValuePair<Guid, ScoreResult>, double>();

            foreach (var kv in scores) {
                double score = kv.Value.FinalScore;
                if (topK.Count < options.MaxDocs) {
                    topK.Enqueue(kv, score);
                } else if (score > topK.Peek().Value.FinalScore) {
                    topK.Dequeue();
                    topK.Enqueue(kv, score);
                }
            }

            var topKHits = topK.UnorderedItems
                .OrderByDescending(k => k.Priority)
                .Select(s => s.Element);

            foreach (var (id, scoreResult) in topKHits) {
                var doc = _docs.Get(id);

                if (doc == null) continue;

                result.Hits.Add(new SearchHit {
                    Document = doc,
                    Explain = explain ? scoreResult : null
                });
            }

            return result;
        }

        public SearchStatus GetStatus(bool onlyRunning = false) {
            if (onlyRunning) return new SearchStatus {
              IsRunning = _isRunning  
            };

            return new SearchStatus {
                IsRunning = _isRunning,
                IsInitialized = _initialized,
                StartetAt = _startedAt,
                TotalDocuments = _docs is null ? 0 : _docs.Count,
                TotalTokens = _tokens is null ? 0 : _tokens.Count,
                TotalNGrams = _nGramIndex is null ? 0 : _nGramIndex.Count
            };
        }
    }
}