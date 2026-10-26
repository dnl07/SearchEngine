namespace SearchEngine.Models.Search
{
    public class SearchHit
    {
        public string Id { get; set; } = "";
        public double Score { get; set; }
        public List<TermMatch>? Explain { get; set; }
    }
}
