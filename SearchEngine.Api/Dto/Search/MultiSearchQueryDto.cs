namespace SearchEngine.Api.Dto.Search
{
    public class MultiSearchQueryDto
    {
        public string Index { get; set; } = "";
        public string Query { get; set; } = "";
        public int? Limit { get; set; }
        public bool Explain { get; set; }
    }
}
