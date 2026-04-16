namespace SearchEngine.Api.Dto.Documents {
    public class DocumentRequestDto {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string[] Tags { get; set; } = [];
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}