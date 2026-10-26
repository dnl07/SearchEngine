namespace SearchEngine.Models.Search
{
    public class IndexQuery
    {
        public string Index { get; set; } = "";
        public string Query { get; set; } = "";
        public int? Limit { get; set; }
        public bool Explain { get; set; }
    }
}
