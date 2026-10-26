using SearchEngine.Core.Ranking;

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

            double idf = Math.Log(1 + (n - df + 0.5) / (df + 0.5));
            double a = 1 - b + b * dl / avdl;
            double expected = idf * tf * (k + 1) / (tf + k * a);

            Assert.Equal(expected, BM25.ComputeScore(tf, df, n, dl, avdl, k, b), 6);
        }

        [Fact]
        public void ComputeScore_TfZero_ReturnsZero()
        {
            Assert.Equal(0, BM25.ComputeScore(tf: 0, df: 10, n: 100, dl: 100, avdl: 100));
        }

        [Fact]
        public void Idf_TermInEveryDocument_IsPositive()
        {
            Assert.True(BM25.Idf(df: 10, n: 10) > 0);
        }

        [Fact]
        public void Idf_RareTerm_IsHigherThanCommonTerm()
        {
            Assert.True(BM25.Idf(df: 1, n: 100) > BM25.Idf(df: 50, n: 100));
        }

        [Fact]
        public void ComputeScore_ShorterField_ScoresHigher()
        {
            Assert.True(
                BM25.ComputeScore(tf: 1, df: 5, n: 100, dl: 2, avdl: 4)
                    > BM25.ComputeScore(tf: 1, df: 5, n: 100, dl: 8, avdl: 4)
            );
        }
    }
}
