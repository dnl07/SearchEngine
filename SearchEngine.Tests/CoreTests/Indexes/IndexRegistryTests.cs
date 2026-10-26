using SearchEngine.Core.Exceptions;
using SearchEngine.Core.Indexes;
using SearchEngine.Models.Search;
using static SearchEngine.Tests.TestIndexes;

namespace SearchEngine.Tests.CoreTests.Indexes
{
    public class IndexRegistryTests
    {
        private readonly IndexRegistry _registry = new();

        [Fact]
        public void CreateOrUpdate_ReportsWhatChanged()
        {
            var changed = ClothingSettings();
            changed.Fields[0].Weight = 4;

            Assert.Equal(
                IndexChange.Created,
                _registry.CreateOrUpdate("clothing", ClothingSettings()).Change
            );
            Assert.Equal(
                IndexChange.Unchanged,
                _registry.CreateOrUpdate("clothing", ClothingSettings()).Change
            );
            Assert.Equal(
                IndexChange.SettingsChanged,
                _registry.CreateOrUpdate("clothing", changed).Change
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData("Clothing")]
        [InlineData("my index")]
        [InlineData("-clothing")]
        public void CreateOrUpdate_InvalidName_Throws(string name)
        {
            Assert.Throws<InvalidInputException>(() =>
                _registry.CreateOrUpdate(name, ClothingSettings())
            );
        }

        [Fact]
        public void Get_UnknownIndex_Throws()
        {
            Assert.Throws<IndexNotFoundException>(() => _registry.Get("missing"));
        }

        [Fact]
        public void Delete_RemovesIndex()
        {
            _registry.CreateOrUpdate("clothing", ClothingSettings());

            Assert.True(_registry.Delete("clothing"));
            Assert.False(_registry.Delete("clothing"));
            Assert.Throws<IndexNotFoundException>(() => _registry.Get("clothing"));
        }

        [Fact]
        public void Indexes_AreSearchedSeparately()
        {
            var clothing = _registry.CreateOrUpdate("clothing", ClothingSettings()).Index;
            var outfits = _registry.CreateOrUpdate("outfits", ClothingSettings()).Index;

            clothing.Upsert([Doc("jacket", category: "Jacket")]);
            outfits.Upsert([Doc("winter", tags: "Winter")]);

            Assert.Equal(new[] { "jacket" }, clothing.SearchIds("jacket"));
            Assert.Empty(outfits.SearchIds("jacket"));
        }

        [Fact]
        public void MultiSearch_GroupsResultsByIndexInQueryOrder()
        {
            _registry
                .CreateOrUpdate("clothing", ClothingSettings())
                .Index.Upsert([
                    Doc("1", category: "Jacket", tags: "Winter"),
                    Doc("2", category: "Pants"),
                ]);
            _registry
                .CreateOrUpdate("outfits", ClothingSettings())
                .Index.Upsert([Doc("a", tags: "Winter")]);

            var results = _registry.MultiSearch([
                new IndexQuery { Index = "outfits", Query = "winter" },
                new IndexQuery { Index = "clothing", Query = "winter" },
            ]);

            Assert.Collection(
                results,
                outfits =>
                {
                    Assert.Equal("outfits", outfits.Index);
                    Assert.Equal(new[] { "a" }, outfits.Result.Hits.Select(hit => hit.Id));
                },
                clothing =>
                {
                    Assert.Equal("clothing", clothing.Index);
                    Assert.Equal(new[] { "1" }, clothing.Result.Hits.Select(hit => hit.Id));
                }
            );
        }

        [Fact]
        public void MultiSearch_UnknownIndex_Throws()
        {
            _registry.CreateOrUpdate("clothing", ClothingSettings());

            Assert.Throws<IndexNotFoundException>(() =>
                _registry.MultiSearch([
                    new IndexQuery { Index = "clothing", Query = "jacket" },
                    new IndexQuery { Index = "missing", Query = "jacket" },
                ])
            );
        }
    }
}
