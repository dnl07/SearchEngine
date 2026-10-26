namespace SearchEngine.Api.Dto.Documents
{
    public class DocumentDto
    {
        public string Id { get; set; } = "";
        public Dictionary<string, string[]> Fields { get; set; } = new();
    }
}
