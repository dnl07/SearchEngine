using SearchEngine.Models.Scoring;

namespace SearchEngine.Api.Dto.Search
{
    public class SearchExplainDto
    {
        public double FinalScore { get; set; }
        public Dictionary<string, List<ScoreContribution>> Contributions { get; set; } = new();
    }
}
