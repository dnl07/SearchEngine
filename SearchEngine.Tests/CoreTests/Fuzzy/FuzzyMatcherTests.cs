using Xunit;

namespace SearchEngine.Tests.CoreTests.Fuzzy
{
    public class FuzzyMatcherTests
    {
        [Theory]
        [InlineData("ranking", new string[] { })]
        [InlineData("searc", new string[] { "search" })]
        [InlineData("apiii", new string[] { "api", "apis" })]
        public async void Match_VariousToken_ReturnsExpectedTokens(
            string token,
            string[] expectedTokens
        )
        {
            var docs = await TestHelper.GetDocumentsFromJson("TestDocuments/documents.json");

            var ctx = TestHelper.CreateFromDocs(docs);

            var matcher = ctx.FuzzyMatcher;

            var tokens = matcher.Match(token, ctx.TokenRegistry, ctx.QueryOptions);
            string[] actualTokens = tokens.Select(t => t.Token).ToArray();

            Assert.Equal(expectedTokens, actualTokens);
        }
    }
}
