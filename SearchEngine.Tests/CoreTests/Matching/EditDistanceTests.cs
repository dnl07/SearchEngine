using SearchEngine.Core.Matching;

namespace SearchEngine.Tests.CoreTests.Matching
{
    public class EditDistanceTests
    {
        [Theory]
        [InlineData("", "", 0)]
        [InlineData("abc", "", 3)]
        [InlineData("jacket", "jacket", 0)]
        [InlineData("jacket", "jaket", 1)]
        [InlineData("jacket", "jakket", 1)]
        [InlineData("kitten", "sitting", 3)]
        [InlineData("sweatshirt", "sweatshrit", 1)]
        [InlineData("nike", "nkie", 1)]
        [InlineData("ab", "ba", 1)]
        [InlineData("jacket", "jakcet", 1)]
        public void Calculate_ReturnsDistanceCountingSwapsAsOneEdit(
            string t1,
            string t2,
            int expected
        )
        {
            Assert.Equal(expected, EditDistance.Calculate(t1, t2));
            Assert.Equal(expected, EditDistance.Calculate(t2, t1));
        }

        [Theory]
        [InlineData("kitten", "sitting", 1, 2)]
        [InlineData("abc", "abcdef", 2, 3)]
        [InlineData("jacket", "jaket", 1, 1)]
        public void Calculate_WithMaxDistance_StopsAboveIt(
            string t1,
            string t2,
            int maxDistance,
            int expected
        )
        {
            Assert.Equal(expected, EditDistance.Calculate(t1, t2, maxDistance));
        }
    }
}
