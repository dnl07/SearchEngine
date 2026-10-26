using SearchEngine.Core.Analysis;
using SearchEngine.Models.Indexes;

namespace SearchEngine.Tests.CoreTests.Analysis
{
    public class StopwordsProviderTests
    {
        private static HashSet<string> GermanAndEnglish(params string[] exclude) =>
            StopwordsProvider.Resolve(
                new StopwordSettings { Languages = ["de", "en"], Exclude = [.. exclude] }
            );

        [Theory]
        [InlineData("und")]
        [InlineData("dass")]
        [InlineData("uber")]
        [InlineData("the")]
        [InlineData("with")]
        public void Resolve_GermanAndEnglish_ContainsFunctionWords(string word)
        {
            Assert.Contains(word, GermanAndEnglish());
        }

        [Theory]
        [InlineData("s")]
        [InlineData("m")]
        [InlineData("l")]
        [InlineData("t")]
        [InlineData("lang")]
        [InlineData("kurz")]
        [InlineData("neu")]
        [InlineData("rot")]
        [InlineData("weiss")]
        public void Resolve_GermanAndEnglish_KeepsClothingWords(string word)
        {
            Assert.DoesNotContain(word, GermanAndEnglish());
        }

        [Fact]
        public void Resolve_WithExclude_RemovesNormalizedWords()
        {
            var stopwords = GermanAndEnglish("Für", "down");

            Assert.DoesNotContain("fur", stopwords);
            Assert.DoesNotContain("down", stopwords);
        }

        [Fact]
        public void Resolve_WithoutLanguages_IsEmpty()
        {
            Assert.Empty(StopwordsProvider.Resolve(new StopwordSettings()));
        }
    }
}
