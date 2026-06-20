namespace SearchEngine.Api.Dto.Search
{
    public class SearchResponseDto
    {
        public string Query { get; set; } = "";
        public int Total { get; set; } = 0;
        public long ElapsedTime { get; set; } = 0;
        public List<string>? MatchedTokens { get; set; } = null;
        public SearchHitDto[] Hits { get; set; } = [];
    }
}
