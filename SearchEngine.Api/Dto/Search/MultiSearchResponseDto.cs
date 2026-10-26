namespace SearchEngine.Api.Dto.Search
{
    public class MultiSearchResponseDto
    {
        public IndexSearchResultDto[] Results { get; set; } = [];
        public long TookMs { get; set; }
    }
}
