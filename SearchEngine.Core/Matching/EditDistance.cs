namespace SearchEngine.Core.Matching
{
    public static class EditDistance
    {
        public static int Calculate(string t1, string t2) =>
            Calculate(t1, t2, Math.Max(t1.Length, t2.Length));

        /// <summary>
        /// Damerau-Levenshtein distance of two tokens, so swapping two
        /// neighbouring letters counts as one edit. Stops early and returns maxDistance + 1 as soon
        /// as the distance is known to exceed maxDistance.
        /// </summary>
        public static int Calculate(string t1, string t2, int maxDistance)
        {
            if (Math.Abs(t1.Length - t2.Length) > maxDistance)
                return maxDistance + 1;

            var beforePrevious = new int[t2.Length + 1];
            var previous = new int[t2.Length + 1];
            var current = new int[t2.Length + 1];

            for (int j = 0; j <= t2.Length; j++)
            {
                previous[j] = j;
            }

            for (int i = 1; i <= t1.Length; i++)
            {
                current[0] = i;
                int rowMinimum = i;

                for (int j = 1; j <= t2.Length; j++)
                {
                    int replacement = previous[j - 1] + (t1[i - 1] == t2[j - 1] ? 0 : 1);
                    current[j] = Math.Min(Math.Min(current[j - 1], previous[j]) + 1, replacement);

                    if (i > 1 && j > 1 && t1[i - 1] == t2[j - 2] && t1[i - 2] == t2[j - 1])
                    {
                        current[j] = Math.Min(current[j], beforePrevious[j - 2] + 1);
                    }

                    rowMinimum = Math.Min(rowMinimum, current[j]);
                }

                if (rowMinimum > maxDistance)
                    return maxDistance + 1;

                (beforePrevious, previous, current) = (previous, current, beforePrevious);
            }

            return Math.Min(previous[t2.Length], maxDistance + 1);
        }
    }
}
