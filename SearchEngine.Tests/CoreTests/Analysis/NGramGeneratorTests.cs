using SearchEngine.Core.Analysis;
using Xunit;

namespace SearchEngine.Tests.CoreTests
{
    public class NGramGeneratorTests
    {
        [Theory]
        [InlineData("", 0, new string[] { })]
        [InlineData("", 1, new string[] { })]
        [InlineData("gram", 1, new[] { "g", "r", "a", "m" })]
        [InlineData("gram", 2, new[] { "$g", "gr", "ra", "am", "m$" })]
        [InlineData("gram", 3, new[] { "$$g", "$gr", "gra", "ram", "am$", "m$$" })]
        [InlineData(
            "generate",
            4,
            new[]
            {
                "$$$g",
                "$$ge",
                "$gen",
                "gene",
                "ener",
                "nera",
                "erat",
                "rate",
                "ate$",
                "te$$",
                "e$$$",
            }
        )]
        public void Generate_VariousNGrams_ReturnsExpectedNGrams(
            string token,
            int nGramSize,
            IEnumerable<string> expected
        )
        {
            var nGrams = NGramGenerator.Generate(token, nGramSize);

            Assert.Equal(expected, nGrams);
        }
    }
}
