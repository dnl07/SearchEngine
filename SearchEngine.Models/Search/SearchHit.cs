using SearchEngine.Core.Documents;
using SearchEngine.Models.Scoring;

namespace SearchEngine.Models.Search
{
    public class SearchHit
    {
        public SearchDocument Document { get; set; } = new();
        public ScoreResult? Explain { get; set; } = null;
    }
}
