using SearchEngine.Core.Documents;
using SearchEngine.Core.Indexing;
using SearchEngine.Core.Matching;

namespace SearchEngine.Tests.CoreTests.Matching
{
    public class CandidateMatcherTests
    {
        private static readonly string[] Vocabulary =
        [
            "coat",
            "raincoat",
            "jacket",
            "jeans",
            "pants",
            "sweatpants",
            "rose",
            "red",
            "darkred",
            "black",
            "sweatshirt",
            "shirt",
            "tshirt",
            "skirt",
            "nike",
            "mint",
            "miniskirt",
            "dress",
            "dresses",
            "boots",
        ];

        [Fact]
        public void Match_FindsExactlyTheTokensABruteForceSearchFinds()
        {
            var matcher = MatcherFor(Vocabulary);

            Assert.All(
                Vocabulary.SelectMany(Variants).Distinct(),
                query =>
                {
                    var expected = Vocabulary
                        .Select(token => (token, typos: ExpectedTypos(query, token)))
                        .Where(match => match.typos is not null)
                        .OrderBy(match => match.token)
                        .Select(match => $"{match.token}:{match.typos}");

                    var actual = matcher
                        .Match(query)
                        .OrderBy(candidate => candidate.Token)
                        .Select(candidate => $"{candidate.Token}:{candidate.Typos}");

                    Assert.Equal(expected, actual);
                }
            );
        }

        private static int? ExpectedTypos(string query, string token)
        {
            bool withoutTypo =
                token == query
                || (query.Length >= CandidateMatcher.MinPrefixLength && token.StartsWith(query))
                || (query.Length >= CandidateMatcher.MinInfixLength && token.Contains(query));

            if (withoutTypo)
                return 0;

            int maxDistance = CandidateMatcher.MaxEditDistance(query.Length);
            int distance = EditDistance.Calculate(query, token, maxDistance);

            return distance > 0 && distance <= maxDistance ? distance : null;
        }

        private static IEnumerable<string> Variants(string word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                yield return word.Remove(i, 1);
                yield return word.Insert(i, "x");
                yield return word[..i] + "q" + word[(i + 1)..];

                if (i < word.Length - 1)
                    yield return word[..i] + word[i + 1] + word[i] + word[(i + 2)..];

                if (i >= 2)
                    yield return word[..i];

                if (i >= 1 && word.Length - i >= 3)
                    yield return word[i..];
            }

            yield return word[..^1] + "xy";
        }

        private static CandidateMatcher MatcherFor(IEnumerable<string> tokens)
        {
            var invertedIndex = new InvertedIndex(1);
            var nGramIndex = new NGramIndex();

            foreach (var (token, i) in tokens.Select((token, i) => (token, i)))
            {
                foreach (
                    var added in invertedIndex.AddDocument(
                        new IndexedDocument(
                            $"{i}",
                            [
                                [token],
                            ]
                        )
                    )
                )
                {
                    nGramIndex.AddToken(added);
                }
            }

            return new CandidateMatcher(invertedIndex, nGramIndex);
        }
    }
}
