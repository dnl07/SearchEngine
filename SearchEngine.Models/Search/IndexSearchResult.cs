namespace SearchEngine.Models.Search
{
    public class IndexSearchResult
    {
        public string Index { get; set; } = "";
        public SearchResult Result { get; set; } = new();
    }
}
