namespace SearchEngine.Models.Search
{
    public class TermMatch
    {
        public string Term { get; set; } = "";
        public string Token { get; set; } = "";
        public MatchKind Kind { get; set; }
        public double Boost { get; set; }
        public int Typos { get; set; }
        public double Score { get; set; }
        public Dictionary<string, double> Fields { get; set; } = new();
    }
}
