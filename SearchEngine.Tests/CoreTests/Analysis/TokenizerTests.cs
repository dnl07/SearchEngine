using SearchEngine.Core.Analysis;

namespace SearchEngine.Tests.CoreTests.Analysis
{
    public class TokenizerTests
    {
        private static readonly HashSet<string> NoStopwords = new();

        [Theory]
        [InlineData("", new string[] { })]
        [InlineData("Red Jacket", new[] { "red", "jacket" })]
        [InlineData("Grün", new[] { "grun" })]
        [InlineData("Weiß", new[] { "weiss" })]
        [InlineData("T-Shirt", new[] { "t", "shirt", "tshirt" })]
        [InlineData("Levi's", new[] { "levi", "s", "levis" })]
        [InlineData("W32/L34", new[] { "w32", "l34", "w32l34" })]
        [InlineData("(Jacket), Pants!", new[] { "jacket", "pants" })]
        public void TokenizeForIndex_VariousInputs_ReturnsPartsAndJoinedForms(
            string text,
            string[] expected
        )
        {
            Assert.Equal(expected, Tokenizer.TokenizeForIndex(text, NoStopwords));
        }

        [Theory]
        [InlineData("   ", new string[] { })]
        [InlineData("T-Shirt", new[] { "tshirt" })]
        [InlineData("red  jacket red", new[] { "red", "jacket" })]
        [InlineData("jacket,pants", new[] { "jacket", "pants" })]
        [InlineData("WEISS", new[] { "weiss" })]
        public void TokenizeQuery_VariousInputs_ReturnsOneTermPerWord(
            string text,
            string[] expected
        )
        {
            Assert.Equal(expected, Tokenizer.TokenizeQuery(text, NoStopwords));
        }

        [Fact]
        public void Tokenize_WithStopwords_RemovesThem()
        {
            var stopwords = new HashSet<string> { "and", "t" };

            Assert.Equal(
                new[] { "shirt", "tshirt", "pants" },
                Tokenizer.TokenizeForIndex("T-Shirt and Pants", stopwords)
            );
            Assert.Equal(
                new[] { "tshirt", "pants" },
                Tokenizer.TokenizeQuery("T-Shirt and Pants", stopwords)
            );
        }
    }
}
