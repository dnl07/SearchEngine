using SearchEngine.Core.Analysis;

namespace SearchEngine.Tests.CoreTests.Analysis
{
    public class NGramGeneratorTests
    {
        [Fact]
        public void Padded_ReturnsNGramsWithEdgesOnBothSides()
        {
            Assert.Equal(
                new[] { "$$j", "$je", "jea", "ean", "ans", "ns$", "s$$" },
                NGramGenerator.Padded("jeans", 3)
            );
        }

        [Fact]
        public void PrefixPadded_ReturnsNGramsWithEdgeAtTheStart()
        {
            Assert.Equal(new[] { "$$j", "$je" }, NGramGenerator.PrefixPadded("je", 3));
        }

        [Theory]
        [InlineData("jacket", new[] { "jac", "ack", "cke", "ket" })]
        [InlineData("je", new string[] { })]
        public void Unpadded_ReturnsInnerNGrams(string token, string[] expected)
        {
            Assert.Equal(expected, NGramGenerator.Unpadded(token, 3));
        }
    }
}
