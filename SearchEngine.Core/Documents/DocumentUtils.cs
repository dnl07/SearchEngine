using SearchEngine.Core.Analysis;
using SearchEngine.Models.Indexing;

namespace SearchEngine.Core.Documents
{
    public static class DocumentUtils
    {
        public static void Tokenize(this SearchDocument doc, HashSet<string>? stopWords = null)
        {
            doc.TitleTokens = Tokenizer.Tokenize(doc.Title, stopWords);
            doc.DescriptionTokens = Tokenizer.Tokenize(doc.Description, stopWords);
            doc.TagsTokens = Tokenizer.Tokenize(string.Join(" ", doc.Tags), stopWords);
            doc.AllTokens = doc.GetAllTokens();
        }

        public static string[] GetFieldTokens(this SearchDocument doc, Field field)
        {
            return field switch
            {
                Field.Title => doc.TitleTokens,
                Field.Description => doc.DescriptionTokens,
                Field.Tags => doc.TagsTokens,
                _ => [],
            };
        }

        private static string[] GetAllTokens(this SearchDocument doc)
        {
            return doc.TitleTokens.Concat(doc.DescriptionTokens).Concat(doc.TagsTokens).ToArray();
        }
    }
}
