namespace SearchEngine.Models.Indexing
{
    public class TokenPosting
    {
        public List<Guid> DocIds { get; set; } = new();
        public List<short> Fields { get; set; } = new();
        public List<int> Frequencies { get; set; } = new();
    }
}
