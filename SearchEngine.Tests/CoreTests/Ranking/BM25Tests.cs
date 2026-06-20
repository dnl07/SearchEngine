using SearchEngine.Core.Ranking;
using Xunit;

namespace SearchEngine.Tests.CoreTests.Ranking
{
    public class BM25Tests
    {
        [Fact]
        public void ComputeScore_ReturnsExpectedValue()
        {
            int tf = 3;
            int df = 10;
            int n = 100;
            int dl = 120;
            double avdl = 100;
            double k = 1.2;
            double b = 0.75;

            double idf = Math.Log2((double)n / df);
            double a = 1 - b + b * dl / avdl;
            double expected = tf * (k + 1) * idf / (k * a + tf);

            double result = BM25.ComputeScore(tf, df, n, dl, avdl, k, b);

            Assert.Equal(expected, result, 6);
        }

        [Fact]
        public void ComputeScore_TfZero_ReturnsZero()
        {
            int tf = 0;
            double score = BM25.ComputeScore(
                tf,
                df: 10,
                n: 100,
                dl: 100,
                avdl: 100,
                k: 1.2,
                b: 0.75
            );

            Assert.Equal(0, score, 6);
        }

        [Fact]
        public void ComputeScore_HigherK_IncreasesScore()
        {
            int tf = 2;
            int df = 5;
            int n = 100;
            int dl = 80;
            double avdl = 100;

            double smallK = BM25.ComputeScore(tf, df, n, dl, avdl, k: 0.5, b: 0.75);
            double bigK = BM25.ComputeScore(tf, df, n, dl, avdl, k: 2.0, b: 0.75);

            Assert.True(bigK > smallK);
        }

        [Fact]
        public void ComputeScore_BetweenZeroAndInfinity()
        {
            double score = BM25.ComputeScore(
                tf: 5,
                df: 10,
                n: 100,
                dl: 90,
                avdl: 100,
                k: 1.2,
                b: 0.75
            );

            Assert.True(score > 0);
            Assert.True(double.IsFinite(score));
        }
    }
}
