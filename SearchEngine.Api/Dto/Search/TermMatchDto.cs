using SearchEngine.Models.Search;

namespace SearchEngine.Api.Dto.Search
{
    public class TermMatchDto
    {
        public string Term { get; set; } = "";
        public string Token { get; set; } = "";
        public MatchKind Kind { get; set; }
        public double Boost { get; set; }
        public double Score { get; set; }
        public Dictionary<string, double> Fields { get; set; } = new();
    }
}
