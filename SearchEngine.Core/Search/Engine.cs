using SearchEngine.Core.Documents;
using SearchEngine.Core.Exceptions;
using SearchEngine.Core.Indexes;
using SearchEngine.Core.Indexing;
using SearchEngine.Core.Matching;
using SearchEngine.Core.Ranking;
using SearchEngine.Models.Search;

namespace SearchEngine.Core.Search
{
    /// <summary>
    /// The data structures of a single index. Not thread-safe, see <see cref="SearchIndex"/>.
    /// </summary>
    public class Engine
    {
        private readonly IndexSchema _schema;
        private readonly DocumentStore _docs;
        private readonly InvertedIndex _invertedIndex;
        private readonly NGramIndex _nGramIndex = new();
        private readonly CandidateMatcher _matcher;

        public Engine(IndexSchema schema)
        {
            _schema = schema;
            _docs = new DocumentStore(schema.FieldNames.Count);
            _invertedIndex = new InvertedIndex(schema.FieldNames.Count);
            _matcher = new CandidateMatcher(_invertedIndex, _nGramIndex);
        }

        public int Count => _docs.Count;

        public void Upsert(IndexedDocument doc)
        {
            Remove(doc.Id);

            _docs.Add(doc);

            foreach (var token in _invertedIndex.AddDocument(doc))
            {
                _nGramIndex.AddToken(token);
            }
        }

        public bool Remove(string id)
        {
            var doc = _docs.Remove(id);

            if (doc is null)
                return false;

            foreach (var token in _invertedIndex.RemoveDocument(doc))
            {
                _nGramIndex.RemoveToken(token);
            }

            return true;
        }

        /// <summary>
        /// Returns the documents matching every term of the query. Documents with fewer typos
        /// come first, then the ones with the higher sum of the best candidate score per term.
        /// </summary>
        public SearchResult Search(string? query, int? limit, bool explain)
        {
            if (limit < 0)
                throw new InvalidInputException("The limit must not be negative.");

            Dictionary<string, List<TermScore>>? matches = null;

            foreach (var term in _schema.AnalyzeQuery(query))
            {
                var termScores = ScoreTerm(term, matches);

                if (matches is null)
                {
                    matches = termScores.ToDictionary(
                        score => score.Key,
                        score => new List<TermScore> { score.Value },
                        StringComparer.Ordinal
                    );
                }
                else
                {
                    foreach (var id in matches.Keys.ToList())
                    {
                        if (termScores.TryGetValue(id, out var score))
                        {
                            matches[id].Add(score);
                        }
                        else
                        {
                            matches.Remove(id);
                        }
                    }
                }

                if (matches.Count == 0)
                    break;
            }

            if (matches is null)
                return new SearchResult();

            var hits = matches
                .Select(match => new
                {
                    Typos = match.Value.Sum(score => score.Candidate.Typos),
                    Hit = new SearchHit
                    {
                        Id = match.Key,
                        Score = match.Value.Sum(score => score.Score),
                        Explain = explain ? match.Value.Select(ToTermMatch).ToList() : null,
                    },
                })
                .OrderBy(match => match.Typos)
                .ThenByDescending(match => match.Hit.Score)
                .ThenBy(match => match.Hit.Id, StringComparer.Ordinal)
                .Take(limit ?? int.MaxValue)
                .Select(match => match.Hit)
                .ToList();

            return new SearchResult { Total = matches.Count, Hits = hits };
        }

        private Dictionary<string, TermScore> ScoreTerm(
            string term,
            Dictionary<string, List<TermScore>>? remaining
        )
        {
            var best = new Dictionary<string, TermScore>(StringComparer.Ordinal);

            foreach (var candidate in _matcher.Match(term))
            {
                var posting = _invertedIndex.Get(candidate.Token);

                if (posting is null)
                    continue;

                var fieldScoresByDoc = new Dictionary<string, double[]>(StringComparer.Ordinal);
                int df = posting.DocumentCount;

                for (int field = 0; field < posting.Fields.Length; field++)
                {
                    var docs = posting.Fields[field];

                    if (docs is null)
                        continue;

                    double avdl = _docs.GetAverageFieldLength(field);
                    double b = _schema.LengthNormalizations[field];

                    foreach (var (id, tf) in docs)
                    {
                        if (remaining is not null && !remaining.ContainsKey(id))
                            continue;

                        if (!fieldScoresByDoc.TryGetValue(id, out var fieldScores))
                        {
                            fieldScores = new double[posting.Fields.Length];
                            fieldScoresByDoc[id] = fieldScores;
                        }

                        int dl = _docs.GetFieldLength(id, field);
                        fieldScores[field] =
                            _schema.Weights[field]
                            * BM25.ComputeScore(tf, df, _docs.Count, dl, avdl, b: b);
                    }
                }

                foreach (var (id, fieldScores) in fieldScoresByDoc)
                {
                    var score = new TermScore(term, candidate, fieldScores);

                    if (!best.TryGetValue(id, out var current) || score.IsBetterThan(current))
                    {
                        best[id] = score;
                    }
                }
            }

            return best;
        }

        private TermMatch ToTermMatch(TermScore score) =>
            new()
            {
                Term = score.Term,
                Token = score.Candidate.Token,
                Kind = score.Candidate.Kind,
                Boost = score.Candidate.Boost,
                Typos = score.Candidate.Typos,
                Score = score.Score,
                Fields = score
                    .FieldScores.Select(
                        (fieldScore, field) =>
                            (
                                Name: _schema.FieldNames[field],
                                Score: fieldScore * score.Candidate.Boost
                            )
                    )
                    .Where(field => field.Score > 0)
                    .ToDictionary(field => field.Name, field => field.Score),
            };

        private sealed record TermScore(string Term, Candidate Candidate, double[] FieldScores)
        {
            public double Score { get; } = FieldScores.Sum() * Candidate.Boost;

            public bool IsBetterThan(TermScore other) =>
                Candidate.Typos != other.Candidate.Typos
                    ? Candidate.Typos < other.Candidate.Typos
                    : Score > other.Score;
        }
    }
}
