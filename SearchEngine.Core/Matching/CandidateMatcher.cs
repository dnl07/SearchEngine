using SearchEngine.Core.Analysis;
using SearchEngine.Core.Indexing;
using SearchEngine.Models.Search;

namespace SearchEngine.Core.Matching
{
    public class CandidateMatcher
    {
        public const double ExactBoost = 1.0;
        public const double PrefixBoost = 0.7;
        public const double InfixBoost = 0.5;
        public const double FuzzyDistanceOneBoost = 0.6;
        public const double FuzzyDistanceTwoBoost = 0.35;

        public const int MinPrefixLength = 2;
        public const int MinInfixLength = 3;

        private readonly InvertedIndex _invertedIndex;
        private readonly NGramIndex _nGramIndex;

        public CandidateMatcher(InvertedIndex invertedIndex, NGramIndex nGramIndex)
        {
            _invertedIndex = invertedIndex;
            _nGramIndex = nGramIndex;
        }

        /// <summary>
        /// Returns how many typos a term of this length may have.
        /// </summary>
        public static int MaxEditDistance(int termLength) =>
            termLength switch
            {
                <= 3 => 0,
                <= 7 => 1,
                _ => 2,
            };

        /// <summary>
        /// Finds all indexed tokens the term can stand for, each with the strongest way it matches.
        /// </summary>
        public IReadOnlyCollection<Candidate> Match(string term)
        {
            var candidates = new Dictionary<string, Candidate>(StringComparer.Ordinal);

            if (_invertedIndex.Contains(term))
            {
                Offer(candidates, new Candidate(term, MatchKind.Exact, ExactBoost));
            }

            if (term.Length >= MinPrefixLength)
            {
                foreach (
                    var token in TokensWithAll(NGramGenerator.PrefixPadded(term, NGramIndex.Size))
                )
                {
                    if (token.StartsWith(term, StringComparison.Ordinal))
                    {
                        Offer(candidates, new Candidate(token, MatchKind.Prefix, PrefixBoost));
                    }
                }
            }

            if (term.Length >= MinInfixLength)
            {
                foreach (var token in TokensWithAll(NGramGenerator.Unpadded(term, NGramIndex.Size)))
                {
                    if (token.Contains(term, StringComparison.Ordinal))
                    {
                        Offer(candidates, new Candidate(token, MatchKind.Infix, InfixBoost));
                    }
                }
            }

            foreach (var candidate in FuzzyCandidates(term))
            {
                Offer(candidates, candidate);
            }

            return candidates.Values;
        }

        /// <summary>
        /// Returns the tokens within the allowed number of typos, filtered by shared n-grams before
        /// the edit distance is computed.
        /// </summary>
        private IEnumerable<Candidate> FuzzyCandidates(string term)
        {
            int maxDistance = MaxEditDistance(term.Length);

            if (maxDistance == 0)
                yield break;

            var overlaps = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var nGram in NGramGenerator.Padded(term, NGramIndex.Size).Distinct())
            {
                foreach (var token in _nGramIndex.GetTokens(nGram))
                {
                    overlaps[token] = overlaps.GetValueOrDefault(token) + 1;
                }
            }

            foreach (var (token, overlap) in overlaps)
            {
                if (Math.Abs(token.Length - term.Length) > maxDistance)
                    continue;

                var nGramCount = Math.Max(term.Length, token.Length) + NGramIndex.Size - 1;
                const int nGramsChangedPerEdit = NGramIndex.Size + 1;

                if (overlap < nGramCount - nGramsChangedPerEdit * maxDistance)
                    continue;

                int distance = EditDistance.Calculate(term, token, maxDistance);

                if (distance == 0 || distance > maxDistance)
                    continue;

                double boost = distance == 1 ? FuzzyDistanceOneBoost : FuzzyDistanceTwoBoost;
                yield return new Candidate(token, MatchKind.Fuzzy, boost, distance);
            }
        }

        /// <summary>
        /// Returns the tokens that contain all of the n-grams.
        /// </summary>
        private IEnumerable<string> TokensWithAll(string[] nGrams)
        {
            var sets = nGrams
                .Distinct()
                .Select(_nGramIndex.GetTokens)
                .OrderBy(tokens => tokens.Count)
                .ToList();

            if (sets.Count == 0)
                return [];

            return sets[0].Where(token => sets.Skip(1).All(tokens => tokens.Contains(token)));
        }

        /// <summary>
        /// Keeps the candidate if it has fewer typos or, with equal typos, a higher boost than the
        /// one already stored for its token.
        /// </summary>
        private static void Offer(Dictionary<string, Candidate> candidates, Candidate candidate)
        {
            if (
                !candidates.TryGetValue(candidate.Token, out var existing)
                || candidate.Typos < existing.Typos
                || (candidate.Typos == existing.Typos && candidate.Boost > existing.Boost)
            )
            {
                candidates[candidate.Token] = candidate;
            }
        }
    }
}
