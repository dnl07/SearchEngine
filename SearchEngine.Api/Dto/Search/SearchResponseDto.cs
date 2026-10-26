namespace SearchEngine.Api.Dto.Search
{
    public class SearchResponseDto
    {
        public int Total { get; set; }
        public SearchHitDto[] Hits { get; set; } = [];
        public long TookMs { get; set; }
    }
}
