namespace SearchEngine.Api.Dto.Search
{
    public class SearchRequestDto
    {
        public string Query { get; set; } = "";
        public QueryOptionsDto Options { get; set; } = new();
    }
}
