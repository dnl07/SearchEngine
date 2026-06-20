namespace SearchEngine.Core.Documents
{
    public class SearchDocument
    {
        public Guid Id { get; set; } = default!;
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string[] Tags { get; set; } = [];
        public Dictionary<string, string> Metadata { get; set; } = new();

        // Cache
        public string[] AllTokens { get; set; } = [];
        public string[] TitleTokens { get; set; } = [];
        public string[] DescriptionTokens { get; set; } = [];
        public string[] TagsTokens { get; set; } = [];
    }
}
