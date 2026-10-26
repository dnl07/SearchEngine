namespace SearchEngine.Models.Documents
{
    public class SearchDocument
    {
        public string Id { get; set; } = "";
        public Dictionary<string, string[]> Fields { get; set; } = new();
    }
}
