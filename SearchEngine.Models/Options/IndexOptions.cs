namespace SearchEngine.Core.Options
{
    public class IndexOptions
    {
        public int NGramSize { get; set; } = 3;
        public StopwordsSource StopwordsSource { get; set; } = StopwordsSource.Default;
        public HashSet<string> StopWords { get; set; } = [""];
        public bool UseOwnIds { get; set; } = false;
    }
}
