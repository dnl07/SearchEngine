namespace SearchEngine.Api.Dto.Search
{
    public class SearchRequestDto
    {
        public string Query { get; set; } = "";
        public int? Limit { get; set; }
        public bool Explain { get; set; }
    }
}
