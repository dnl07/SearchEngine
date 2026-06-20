using SearchEngine.Core.Analysis;
using SearchEngine.Core.Documents;
using SearchEngine.Core.Indexing;
using SearchEngine.Core.Options;

namespace SearchEngine.Core.Fuzzy
{
    public class FuzzyMatcher
    {
        private readonly NGramIndex _nGramIndex;
        private readonly IndexOptions _indexOptions;

        public FuzzyMatcher(NGramIndex nGramIndex, IndexOptions options)
        {
            _nGramIndex = nGramIndex;
            _indexOptions = options;
        }

        /// <summary>
        /// Matches a given token to other token and returns all candidates
        /// </summary>
        public IReadOnlyList<FuzzyToken> Match(
            string token,
            TokenRegistry tokenRegistry,
            QueryOptions queryOptions
        )
        {
            // Dictionary to count how many n-Grams each candidate token shares with the input token
            var candidateCounts = new Dictionary<string, int>();

            var inputNGrams = NGramGenerator.Generate(token, _indexOptions.NGramSize);

            // Generate n-Grams for the input token and find all possible candidates
            foreach (var nGram in inputNGrams)
            {
                foreach (var candidateId in _nGramIndex.GetCandidates(nGram))
                {
                    var candidateToken = tokenRegistry.GetToken(candidateId);

                    if (candidateToken == token)
                        continue;

                    if (candidateCounts.TryGetValue(candidateToken, out var count))
                    {
                        candidateCounts[candidateToken] = count + 1;
                    }
                    else
                    {
                        candidateCounts[candidateToken] = 1;
                    }
                }
            }

            // Preselect candidates
            List<string> preselectedTokens = new List<string>();

            foreach ((string candidateToken, int overlapCount) in candidateCounts)
            {
                var candidateNGrams = _nGramIndex.GetNGrams(
                    tokenRegistry.GetIdOfToken(candidateToken)
                );

                if (
                    overlapCount
                    >= Math.Max(inputNGrams.Length, candidateNGrams.Length)
                        - _indexOptions.NGramSize * queryOptions.MaxEditDistance
                )
                {
                    preselectedTokens.Add(candidateToken);
                }
            }

            // Calculate actual edit distance
            List<FuzzyToken> fuzzyMatches = new List<FuzzyToken>();
            foreach (var preselected in preselectedTokens)
            {
                var editDistance = EditDistance.Calculate(token, preselected);
                if (editDistance <= queryOptions.MaxEditDistance)
                {
                    fuzzyMatches.Add(new FuzzyToken(preselected, editDistance));
                }
            }

            return fuzzyMatches;
        }
    }
}
