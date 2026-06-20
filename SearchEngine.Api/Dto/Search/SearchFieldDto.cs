namespace SearchEngine.Api.Dto.Search
{
    public class SearchFieldDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string[] Tags { get; set; } = [];
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
