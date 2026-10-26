namespace SearchEngine.Core.Ranking
{
    public static class BM25
    {
        public const double DefaultK = 1.2;
        public const double DefaultB = 0.75;

        /// <summary>
        /// Inverse document frequency as used by Lucene. Always positive, even for tokens
        /// contained in every document.
        /// </summary>
        public static double Idf(int df, int n) => Math.Log(1 + (n - df + 0.5) / (df + 0.5));

        /// <summary>
        /// Computes the BM25 for a given posting.
        /// </summary>
        /// <param name="tf">Term frequency.</param>
        /// <param name="df">Number of documents containing the term.</param>
        /// <param name="n">The count of all documents.</param>
        /// <param name="dl">The length of a given document.</param>
        /// <param name="avdl">The average document length of all documents.</param>
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
