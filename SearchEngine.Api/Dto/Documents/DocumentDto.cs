namespace SearchEngine.Api.Dto.Documents
{
    public class DocumentDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Title { get; set; } = "";
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
