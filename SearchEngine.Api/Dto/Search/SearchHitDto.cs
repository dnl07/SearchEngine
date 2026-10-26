namespace SearchEngine.Api.Dto.Search
{
    public class SearchHitDto
    {
        public string Id { get; set; } = "";
        public double Score { get; set; }
        public TermMatchDto[]? Explain { get; set; }
    }
}
