using SearchEngine.Core.Fuzzy;
using Xunit;

namespace SearchEngine.Tests.CoreTests
{
    public class EditDistanceTests
    {
        [Theory]
        [InlineData("", "", 0)]
        [InlineData("edit", "", 4)]
        [InlineData("", "edit", 4)]
        [InlineData("edit", "edited", 2)]
        [InlineData("edit", "ebid", 2)]
        [InlineData("distance", "edit", 6)]
        public void Generate_VariousInputs_ReturnsExpectedEditDistance(
            string token,
            string otherToken,
            int expectedDistance
        )
        {
            var distance = EditDistance.Calculate(token, otherToken);
            Assert.Equal(expectedDistance, distance);
        }
    }
}
