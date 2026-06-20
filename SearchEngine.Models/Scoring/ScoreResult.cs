namespace SearchEngine.Models.Scoring
{
    public class ScoreResult
    {
        public double FinalScore { get; set; } = 0;
        public Dictionary<string, List<ScoreContribution>> Contributions { get; set; } = new();
    }
}
