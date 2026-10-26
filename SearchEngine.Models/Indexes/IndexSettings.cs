namespace SearchEngine.Models.Indexes
{
    public class IndexSettings
    {
        public List<FieldSettings> Fields { get; set; } = new();
        public StopwordSettings Stopwords { get; set; } = new();
    }
}
