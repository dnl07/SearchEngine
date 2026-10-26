namespace SearchEngine.Models.Search
{
    public class SearchResult
    {
        public int Total { get; set; }
        public List<SearchHit> Hits { get; set; } = new();
    }
}
