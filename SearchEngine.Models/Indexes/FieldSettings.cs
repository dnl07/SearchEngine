namespace SearchEngine.Models.Indexes
{
    public class FieldSettings
    {
        public string Name { get; set; } = "";
        public double Weight { get; set; } = 1.0;

        /// <summary>
        /// How much longer field values lower the score (BM25 b), from 0 (not at all) to 1.
        /// Null uses the default of 0.75.
        /// </summary>
        public double? LengthNormalization { get; set; }
    }
}
