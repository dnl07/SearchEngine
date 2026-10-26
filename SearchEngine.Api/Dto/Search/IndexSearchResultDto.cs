namespace SearchEngine.Api.Dto.Search
{
    public class IndexSearchResultDto
    {
        public string Index { get; set; } = "";
        public int Total { get; set; }
        public SearchHitDto[] Hits { get; set; } = [];
    }
}
