using SearchEngine.Core.Documents;
using SearchEngine.Core.Indexing;
using SearchEngine.Core.Options;
using SearchEngine.Models.Indexing;
using SearchEngine.Models.Scoring;

namespace SearchEngine.Core.Ranking
{
    public class ScoringEngine
    {
        public Dictionary<Guid, ScoreResult> ScoreDocuments(
            List<(string token, double boost)> expanded,
            InvertedIndex _invertedIndex,
            DocumentStore docs,
            QueryOptions options,
            bool explain
        )
        {
            var scores = new Dictionary<Guid, ScoreResult>();

            foreach ((string term, double boost) in expanded)
            {
                var posting = _invertedIndex.GetTokenPosting(term);

                if (posting == null)
                    continue;

                int df = posting.DocIds.Count;

                for (int i = 0; i < posting.DocIds.Count; i++)
                {
                    var docId = posting.DocIds[i];
                    var field = FieldOrdinal.OrdinalToField(posting.Fields[i]);
                    var tf = posting.Frequencies[i];

                    double bm25 = ComputeBM25Score(docId, field, tf, df, docs, options);

                    double fieldWeight = field switch
                    {
                        Field.Title => options.TitleWeight,
                        Field.Description => options.DescriptionWeight,
                        Field.Tags => options.TagWeight,
                        _ => 1.0,
                    };

                    double finalScore = bm25 * boost * fieldWeight;

                    if (!scores.TryGetValue(docId, out var score))
                    {
                        score = new ScoreResult();
                        scores[docId] = score;
                    }

                    score.FinalScore += finalScore;

                    if (explain)
                    {
                        if (!score.Contributions.TryGetValue(term, out var contributions))
                        {
                            contributions = new List<ScoreContribution>();
                            score.Contributions[term] = contributions;
                        }

                        contributions.Add(
                            new ScoreContribution
                            {
                                Field = field,
                                TermFrequency = tf,
                                BM25 = bm25,
                                FuzzyBoost = boost,
                                FieldWeight = fieldWeight,
                                Final = finalScore,
                            }
                        );
                    }
                }
            }
            return scores;
        }

        /// <summary>
        /// Calculates the BM25-Score for a posting given all documents.
        /// </summary>
        private static double ComputeBM25Score(
            Guid docId,
            Field field,
            int tf,
            int df,
            DocumentStore docs,
            QueryOptions options
        )
        {
            int n = docs.GetCount(field);

            if (n <= 0 || df <= 0 || df > n)
                return 0.0;

            // Average length of the field
            double avdl = docs.GetAverageFieldLength(field);
            if (avdl <= 0)
                return 0.0;

            var dl = docs.GetFieldLength(docId, field);

            double bm25 = BM25.ComputeScore(tf, df, n, dl, avdl, options.BM25K, options.BM25B);

            if (double.IsNaN(bm25) || double.IsInfinity(bm25))
                return 0.0;

            return bm25;
        }
    }
}
