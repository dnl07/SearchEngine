namespace SearchEngine.Api.Dto.Search
{
    public class SearchHitDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public SearchFieldDto Fields { get; set; } = new();
        public SearchExplainDto? Explain { get; set; } = null;
    }
}
