namespace SearchEngine.Core.Indexing
{
    public class TokenPosting
    {
        public TokenPosting(int fieldCount)
        {
            Fields = new Dictionary<string, int>?[fieldCount];
        }

        /// <summary>
        /// Per field ordinal: the documents containing the token, mapped to the token's frequency.
        /// </summary>
        public Dictionary<string, int>?[] Fields { get; }

        /// <summary>
        /// The number of documents containing the token in any field.
        /// </summary>
        public int DocumentCount { get; set; }

        public bool IsEmpty => Fields.All(docs => docs is null);
    }
}
