using SearchEngine.Core.Analysis;
using SearchEngine.Core.Documents;
using SearchEngine.Core.Options;

namespace SearchEngine.Core.Fuzzy
{
    public class QueryExpander
    {
        public List<(string token, double boost)> Expand(
            string request,
            FuzzyMatcher fuzzyMatcher,
            TokenRegistry tokenRegistry,
            HashSet<string> stopwords,
            QueryOptions options
        )
        {
            var expanded = new List<(string term, double boost)>();

            foreach (string term in Tokenizer.Tokenize(request, stopwords))
            {
                expanded.Add((term, 1.0));

                foreach (
                    var fuzzy in fuzzyMatcher
                        .Match(term, tokenRegistry, options)
                        .OrderBy(f => f.EditDistance)
                        .Take(options.MaxFuzzyExpansions)
                )
                {
                    double boost =
                        term == fuzzy.Token
                            ? options.ExactMatchBoost
                            : Math.Exp(-fuzzy.EditDistance);
                    expanded.Add((fuzzy.Token, boost));
                }
            }
            return expanded;
        }
    }
}
