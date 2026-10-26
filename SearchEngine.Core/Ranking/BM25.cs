namespace SearchEngine.Core.Ranking
{
    public static class BM25
    {
        public const double DefaultK = 1.2;
        public const double DefaultB = 0.75;

        /// <summary>
        /// Returns the inverse document frequency as Lucene computes it, which stays positive even
        /// for tokens in every document.
        /// </summary>
        public static double Idf(int df, int n) => Math.Log(1 + (n - df + 0.5) / (df + 0.5));

        /// <summary>
        /// Computes the BM25 score of a term from its frequency in the field (tf), the number of
        /// documents containing it (df), the document count (n) and the field length (dl) compared
        /// to the average (avdl).
        /// </summary>
        public static double ComputeScore(
            int tf,
            int df,
            int n,
            int dl,
            double avdl,
            double k = DefaultK,
            double b = DefaultB
        )
        {
            if (tf <= 0 || n <= 0 || avdl <= 0)
                return 0.0;

            double a = 1 - b + b * dl / avdl;
            return Idf(df, n) * tf * (k + 1) / (tf + k * a);
        }
    }
}
