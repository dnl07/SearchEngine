using SearchEngine.Core.Documents;
using Xunit;

namespace SearchEngine.Tests.CoreTests
{
    public class DocumentStoreTests
    {
        [Fact]
        public void Adding_Documents_ReturnsExpectedStore()
        {
            var docStore = new DocumentStore();
            var doc = new SearchDocument
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Tags = [],
            };

            docStore.Add(doc);
            var copyDoc = docStore.Get(doc.Id);

            Assert.Equal(1, docStore.Count);
            Assert.Equivalent(doc, copyDoc);
        }

        [Fact]
        public void Removing_Documents_ReturnsExpectedStore()
        {
            var docStore = new DocumentStore();
            var doc = new SearchDocument
            {
                Id = Guid.NewGuid(),
                Title = "Title",
                Description = "Description",
                Tags = [],
            };

            docStore.Add(doc);
            docStore.Remove(doc.Id);

            var nullDoc = docStore.Get(doc.Id);

            Assert.Equal(0, docStore.Count);
            Assert.Null(nullDoc);
        }
    }
}
