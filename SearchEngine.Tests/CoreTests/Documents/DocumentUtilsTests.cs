using SearchEngine.Core.Documents;
using Xunit;

namespace SearchEngine.Tests.CoreTests
{
    public class DocumentUtilsTests
    {
        [Fact]
        public void Tokenize_Document_ReturnsExpectedTokens()
        {
            var doc = new SearchDocument
            {
                Id = Guid.NewGuid(),
                Title = "Title example",
                Description = "Description",
                Tags = ["string", "document", "example"],
            };

            doc.Tokenize();
            string[] titleTokens = ["title", "example"];
            string[] descToken = ["description"];
            string[] tagsTokens = ["string", "document", "example"];
            string[] allTokens = titleTokens.Concat(descToken).Concat(tagsTokens).ToArray();

            Assert.Equal(titleTokens, doc.TitleTokens);
            Assert.Equal(descToken, doc.DescriptionTokens);
            Assert.Equal(tagsTokens, doc.TagsTokens);
            Assert.Equal(allTokens, doc.AllTokens);
        }
    }
}
