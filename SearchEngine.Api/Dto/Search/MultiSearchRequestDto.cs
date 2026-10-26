namespace SearchEngine.Api.Dto.Search
{
    public class MultiSearchRequestDto
    {
        public List<MultiSearchQueryDto> Queries { get; set; } = new();
    }
}
