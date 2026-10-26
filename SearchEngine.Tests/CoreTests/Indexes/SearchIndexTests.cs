using System.Collections.Concurrent;
using SearchEngine.Core.Exceptions;
using SearchEngine.Core.Indexes;
using SearchEngine.Models.Documents;
using SearchEngine.Models.Indexes;
using static SearchEngine.Tests.TestIndexes;

namespace SearchEngine.Tests.CoreTests.Indexes
{
    public class SearchIndexTests
    {
        private static readonly SearchDocument UnknownField = new()
        {
            Id = "unknown",
            Fields = new() { ["unknown"] = ["value"] },
        };

        [Fact]
        public void Upsert_ExistingId_ReplacesDocument()
        {
            var index = Clothing(Doc("1", color: "Red"));

            index.Upsert([Doc("1", color: "Blue")]);

            Assert.Equal(1, index.DocumentCount);
            Assert.Empty(index.SearchIds("red"));
            Assert.Equal(new[] { "1" }, index.SearchIds("blue"));
        }

        [Fact]
        public void Upsert_InvalidDocument_ChangesNothing()
        {
            var index = Clothing(Doc("1", color: "Red"));

            Assert.Throws<InvalidInputException>(() =>
                index.Upsert([Doc("1", color: "Blue"), UnknownField])
            );

            Assert.Equal(new[] { "1" }, index.SearchIds("red"));
        }

        [Fact]
        public void Replace_SwapsAllDocuments()
        {
            var index = Clothing(Doc("1", color: "Red"), Doc("2", color: "Red"));

            int count = index.Replace([Doc("3", color: "Blue")]);

            Assert.Equal(1, count);
            Assert.Empty(index.SearchIds("red"));
            Assert.Equal(new[] { "3" }, index.SearchIds("blue"));
        }

        [Fact]
        public void Replace_InvalidDocument_KeepsOldDocuments()
        {
            var index = Clothing(Doc("1", color: "Red"));

            Assert.Throws<InvalidInputException>(() =>
                index.Replace([Doc("2", color: "Blue"), UnknownField])
            );

            Assert.Equal(new[] { "1" }, index.SearchIds("red"));
        }

        [Fact]
        public void Remove_ExistingDocument_RemovesItsTokens()
        {
            var index = Clothing(Doc("1", color: "Red"), Doc("2", color: "Blue"));

            Assert.True(index.Remove("1"));

            Assert.Equal(1, index.DocumentCount);
            Assert.Empty(index.SearchIds("re"));
        }

        [Fact]
        public void IncrementalWrites_ScoreLikeAFreshlyBuiltIndex()
        {
            var incremental = Clothing(
                Doc("1", tags: "Winter", category: "Jacket", notes: "warm and red"),
                Doc("2", tags: "Summer", category: "Jacket", color: "Red"),
                Doc("3", category: "Pants", notes: "Winter pants"),
                Doc("4", tags: "Winter Sport", category: "Hoodie")
            );
            incremental.Upsert([Doc("2", tags: "Winter", category: "Coat", notes: "long")]);
            incremental.Remove("3");
            incremental.Upsert([
                Doc("5", category: "Jacket", color: "Blue"),
                Doc("4", category: "Hoodie"),
            ]);

            var fresh = Clothing(
                Doc("1", tags: "Winter", category: "Jacket", notes: "warm and red"),
                Doc("2", tags: "Winter", category: "Coat", notes: "long"),
                Doc("4", category: "Hoodie"),
                Doc("5", category: "Jacket", color: "Blue")
            );

            Assert.All(
                new[] { "winter", "jacket", "red", "jac", "wintr", "hoodie", "pants", "sport" },
                query => Assert.Equal(Scores(fresh, query), Scores(incremental, query))
            );
        }

        [Fact]
        public async Task Replace_WhileSearching_ShowsEitherTheOldOrTheNewDocuments()
        {
            var winter = Enumerable
                .Range(0, 50)
                .Select(i => Doc($"w{i}", tags: "Winter Jacket"))
                .ToArray();
            var summer = Enumerable
                .Range(0, 80)
                .Select(i => Doc($"s{i}", tags: "Summer Jacket"))
                .ToArray();
            var index = Clothing(winter);
            var totals = new ConcurrentBag<int>();
            var ct = TestContext.Current.CancellationToken;

            var writer = Task.Run(
                () =>
                {
                    for (int i = 0; i < 50; i++)
                    {
                        index.Replace(i % 2 == 0 ? summer : winter);
                    }
                },
                ct
            );
            var reader = Task.Run(
                () =>
                {
                    do
                    {
                        totals.Add(index.Search("jacket", 0, false).Total);
                    } while (!writer.IsCompleted);
                },
                ct
            );
            await Task.WhenAll(writer, reader);

            Assert.All(totals, total => Assert.Contains(total, new[] { 50, 80 }));
        }

        [Fact]
        public void Remove_UnknownDocument_ReturnsFalse()
        {
            Assert.False(Clothing().Remove("missing"));
        }

        [Fact]
        public void Configure_SameSettings_KeepsDocuments()
        {
            var index = Clothing(Doc("1", color: "Red"));

            Assert.False(index.Configure(IndexSchema.Create(ClothingSettings())));
            Assert.Equal(1, index.DocumentCount);
        }

        [Fact]
        public void Configure_ChangedSettings_ClearsDocuments()
        {
            var index = Clothing(Doc("1", color: "Red"));
            var settings = ClothingSettings();
            settings.Fields.Add(new FieldSettings { Name = "material", Weight = 1.5 });

            Assert.True(index.Configure(IndexSchema.Create(settings)));
            Assert.Equal(0, index.DocumentCount);
        }

        [Fact]
        public async Task ParallelUpsertsAndSearches_KeepIndexConsistent()
        {
            var index = Clothing();
            var ct = TestContext.Current.CancellationToken;

            var writers = Enumerable
                .Range(0, 4)
                .Select(writer =>
                    Task.Run(
                        () =>
                        {
                            for (int i = 0; i < 250; i++)
                            {
                                var color = i % 2 == 0 ? "Red" : "Blue";
                                index.Upsert([
                                    Doc($"{writer}-{i}", category: "Jacket", color: color),
                                ]);
                            }
                        },
                        ct
                    )
                );

            var readers = Enumerable
                .Range(0, 4)
                .Select(_ =>
                    Task.Run(
                        () =>
                        {
                            for (int i = 0; i < 250; i++)
                            {
                                index.Search("red jacket", 10, false);
                            }
                        },
                        ct
                    )
                );

            await Task.WhenAll(writers.Concat(readers));

            Assert.Equal(1000, index.DocumentCount);
            Assert.Equal(500, index.Search("red jacket", null, false).Total);
        }

        private static (string Id, double Score)[] Scores(SearchIndex index, string query) =>
            index
                .Search(query, null, false)
                .Hits.Select(hit => (hit.Id, Math.Round(hit.Score, 9)))
                .ToArray();
    }
}
