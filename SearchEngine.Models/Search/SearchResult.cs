namespace SearchEngine.Models.Search
{
    public class SearchResult
    {
        public string Query { get; set; } = "";
        public List<string>? MatchedTokens { get; set; } = null;
        public List<SearchHit> Hits { get; set; } = new();
    }
}
