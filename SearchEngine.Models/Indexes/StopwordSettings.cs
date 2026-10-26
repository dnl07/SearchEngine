namespace SearchEngine.Models.Indexes
{
    public class StopwordSettings
    {
        public List<string> Languages { get; set; } = new();
        public List<string> Exclude { get; set; } = new();
    }
}
