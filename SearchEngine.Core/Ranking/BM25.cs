namespace SearchEngine.Core.Ranking
{
    public static class BM25
    {
        /// <summary>
        /// Computes the BM25 for a given posting.
        /// </summary>
        /// <param name="tf">Term frequency.</param>
        /// <param name="df">Document frequency.</param>
        /// <param name="n">The count of all documents.</param>
        /// <param name="dl">The length of a given document.</param>
        /// <param name="avdl">The average document length of all documents.</param>
        public static double ComputeScore(
            int tf,
            int df,
            int n,
            int dl,
            double avdl,
            double k,
            double b
        )
        {
            // Inverse document frequency
            double idf = Math.Log2((double)n / df);

            double a = 1 - b + b * dl / avdl;
            return tf * (k + 1) * idf / (k * a + tf);
        }
    }
}
