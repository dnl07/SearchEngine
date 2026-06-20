using SearchEngine.Models.Indexing;

namespace SearchEngine.Models.Scoring
{
    public class ScoreContribution
    {
        public Field Field { get; set; }
        public double FieldWeight { get; set; }
        public int TermFrequency { get; set; }
        public double BM25 { get; set; }
        public double FuzzyBoost { get; set; }
        public double Final { get; set; }
    }
}
