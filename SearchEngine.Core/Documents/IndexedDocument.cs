namespace SearchEngine.Core.Documents
{
    public sealed class IndexedDocument
    {
        public IndexedDocument(string id, string[][] tokens)
        {
            Id = id;
            Tokens = tokens;
        }

        public string Id { get; }

        /// <summary>
        /// The tokens of every field, indexed by the field's ordinal.
        /// </summary>
        public string[][] Tokens { get; }
    }
}
