namespace SearchEngine.Core.Exceptions
{
    public class IndexNotFoundException : Exception
    {
        public IndexNotFoundException(string index)
            : base($"Index '{index}' does not exist.")
        {
            Index = index;
        }

        public string Index { get; }
    }
}
