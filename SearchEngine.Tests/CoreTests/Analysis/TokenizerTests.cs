using SearchEngine.Core.Analysis;
using Xunit;

namespace SearchEngine.Tests.CoreTests
{
    public class TokenizerTests
    {
        [Theory]
        [InlineData("", new string[] { })]
        [InlineData("token", new[] { "token" })]
        [InlineData("this is a test", new[] { "this", "is", "a", "test" })]
        [InlineData("töken", new[] { "token" })]
        [InlineData("tóken", new[] { "token" })]
        [InlineData("!token,!!", new[] { "token" })]
        [InlineData(",token, . test", new[] { "token", "test" })]
        public void Tokenize_VariousInputs_ReturnsExpectedNormalizedString(
            string token,
            string[] expected
        )
        {
            var normalized = Tokenizer.Tokenize(token);

            Assert.Equal(expected, normalized);
        }

        [Theory]
        [InlineData("", new string[] { }, new string[] { })]
        [InlineData("token", new[] { "token" }, new string[] { })]
        [InlineData("token", new[] { "" }, new[] { "token" })]
        [InlineData("this is a test", new[] { "is", "a" }, new[] { "this", "test" })]
        public void TokenizeWithStopWords_VariousInputs_ReturnsExpectedString(
            string token,
            IEnumerable<string> stopwords,
            string[] expected
        )
        {
            var normalized = Tokenizer.Tokenize(token, stopwords.ToHashSet());

            Assert.Equal(expected, normalized);
        }
    }
}
