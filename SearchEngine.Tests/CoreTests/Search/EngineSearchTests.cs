using SearchEngine.Core.Exceptions;
using SearchEngine.Core.Indexes;
using SearchEngine.Models.Documents;
using SearchEngine.Models.Indexes;
using SearchEngine.Models.Search;
using static SearchEngine.Tests.TestIndexes;

namespace SearchEngine.Tests.CoreTests.Search
{
    public class EngineSearchTests
    {
        [Theory]
        [InlineData("red jacket")]
        [InlineData("red jackets")]
        [InlineData("Jacket RED")]
        public void Search_SeveralTerms_RequiresEveryTerm(string query)
        {
            var index = Clothing(
                Doc("1", category: "Jacket", color: "Red"),
                Doc("2", category: "Jacket", color: "Blue"),
                Doc("3", category: "Pants", color: "Red")
            );

            Assert.Equal(new[] { "1" }, index.SearchIds(query));
        }

        [Theory]
        [InlineData("je", "jeans")]
        [InlineData("jac", "jacket")]
        public void Search_StartOfToken_FindsPrefixMatches(string query, string expectedId)
        {
            var index = Clothing(
                Doc("jeans", category: "Jeans"),
                Doc("jacket", category: "Jacket")
            );

            Assert.Equal(new[] { expectedId }, index.SearchIds(query));
        }

        [Fact]
        public void Search_PartOfCompoundWord_FindsInfixMatchesAfterExactMatches()
        {
            var index = Clothing(
                Doc("raincoat", category: "Raincoat"),
                Doc("coat", category: "Coat")
            );

            Assert.Equal(new[] { "coat", "raincoat" }, index.SearchIds("coat"));
        }

        [Fact]
        public void Search_ExactAndPrefixMatch_RanksExactMatchFirst()
        {
            var index = Clothing(Doc("rocky", category: "Rocky"), Doc("rock", category: "Rock"));

            Assert.Equal(new[] { "rock", "rocky" }, index.SearchIds("rock"));
        }

        [Theory]
        [InlineData("jaket", true)]
        [InlineData("jackett", true)]
        [InlineData("jxckxt", false)]
        [InlineData("rad", false)]
        [InlineData("sweatshrit", true)]
        public void Search_Typos_AreToleratedDependingOnLength(string query, bool found)
        {
            var index = Clothing(
                Doc("1", category: "Jacket"),
                Doc("2", color: "Red"),
                Doc("3", category: "Sweatshirt")
            );

            Assert.Equal(found, index.SearchIds(query).Length > 0);
        }

        [Theory]
        [InlineData("m", "m")]
        [InlineData("S", "s")]
        [InlineData("xl", "xl")]
        public void Search_Sizes_AreNotStopwords(string query, string expectedId)
        {
            var index = Clothing(
                Doc("s", size: "S"),
                Doc("m", size: "M"),
                Doc("l", size: "L"),
                Doc("xl", size: "XL")
            );

            Assert.Equal(new[] { expectedId }, index.SearchIds(query));
        }

        [Theory]
        [InlineData("jacket and")]
        [InlineData("the jacket")]
        public void Search_Stopwords_AreIgnored(string query)
        {
            var index = Clothing(Doc("1", category: "Jacket"));

            Assert.Equal(new[] { "1" }, index.SearchIds(query));
        }

        [Theory]
        [InlineData("Weiß", "weiss")]
        [InlineData("weiss", "Weiß")]
        [InlineData("Grün", "grun")]
        [InlineData("Grun", "grün")]
        public void Search_EszettAndDiacritics_AreNormalized(string color, string query)
        {
            var index = Clothing(Doc("1", color: color));

            Assert.Equal(new[] { "1" }, index.SearchIds(query));
        }

        [Theory]
        [InlineData("t-shirt")]
        [InlineData("tshirt")]
        [InlineData("T-Shirt")]
        public void Search_CompoundWord_FindsBothSpellings(string query)
        {
            var index = Clothing(
                Doc("hyphen", category: "T-Shirt"),
                Doc("joined", category: "Tshirt")
            );

            Assert.Equal(new[] { "hyphen", "joined" }, index.SearchIds(query).Order());
        }

        [Theory]
        [InlineData("shirt")]
        [InlineData("t shirt")]
        public void Search_PartsOfCompoundWord_FindIt(string query)
        {
            var index = Clothing(
                Doc("hyphen", category: "T-Shirt"),
                Doc("other", category: "Pants")
            );

            Assert.Equal(new[] { "hyphen" }, index.SearchIds(query));
        }

        [Fact]
        public void Search_TermInDifferentFields_RanksHigherWeightedFieldFirst()
        {
            var index = Clothing(Doc("notes", notes: "winter"), Doc("tags", tags: "winter"));

            Assert.Equal(new[] { "tags", "notes" }, index.SearchIds("winter"));
        }

        [Fact]
        public void Search_TermInFieldThatFewDocumentsHave_StillRanksByFieldWeight()
        {
            var tagged = Enumerable
                .Range(0, 5)
                .Select(i => Doc($"tag-{i}", tags: "Winter", notes: "Comfy and warm"));
            var untagged = Enumerable
                .Range(0, 24)
                .Select(i => Doc($"plain-{i}", notes: "Comfy and warm"));
            var index = Clothing([
                .. tagged,
                .. untagged,
                Doc("notes", notes: "Good for the winter"),
            ]);

            var ids = index.SearchIds("winter");

            Assert.Equal("notes", ids[^1]);
            Assert.All(ids[..^1], id => Assert.StartsWith("tag-", id));
        }

        [Fact]
        public void Search_Typos_RankAfterExactMatchesEvenInHeavierFields()
        {
            var index = Clothing(
                Doc("skirt", category: "Skirt"),
                Doc("shirt-1", category: "T-Shirt"),
                Doc("shirt-2", category: "T-Shirt"),
                Doc("shirt-3", notes: "old shirt")
            );

            var ids = index.SearchIds("shirt");

            Assert.Equal("skirt", ids[^1]);
            Assert.Equal(4, ids.Length);
        }

        [Fact]
        public void Search_PrefixMatch_RanksBeforeTypoInHeavierField()
        {
            var index = Clothing(
                Doc("hose", category: "Hose"),
                Doc("dress", notes: "rosebud pattern")
            );

            Assert.Equal(new[] { "dress", "hose" }, index.SearchIds("rose"));
        }

        [Fact]
        public void Search_DocumentMatchingExactlyAndWithTypo_CountsAsExact()
        {
            var index = Clothing(
                Doc("both", category: "Skirt", notes: "shirt"),
                Doc("typo", category: "Skirt")
            );

            var hits = index.Search("shirt", null, true).Hits;

            Assert.Equal(new[] { "both", "typo" }, hits.Select(hit => hit.Id));
            Assert.Equal(0, Assert.Single(hits[0].Explain!).Typos);
            Assert.Equal(1, Assert.Single(hits[1].Explain!).Typos);
        }

        [Theory]
        [InlineData("nkie")]
        [InlineData("jakcet")]
        public void Search_SwappedLetters_CountAsOneTypo(string query)
        {
            var index = Clothing(Doc("nike", brand: "Nike"), Doc("jacket", category: "Jacket"));

            var hit = Assert.Single(index.Search(query, null, true).Hits);

            Assert.Equal(1, Assert.Single(hit.Explain!).Typos);
        }

        [Fact]
        public void Search_DefaultLengthNormalization_PrefersShorterFields()
        {
            var index = Clothing(
                Doc("one-tag", tags: "Winter"),
                new SearchDocument
                {
                    Id = "five-tags",
                    Fields = new() { ["tags"] = ["Winter", "Sport", "Party", "Work", "Vacation"] },
                }
            );

            var hits = index.Search("winter", null, false).Hits;

            Assert.Equal("one-tag", hits[0].Id);
            Assert.True(hits[0].Score > hits[1].Score);
        }

        [Fact]
        public void Search_WithoutLengthNormalization_IgnoresFieldLength()
        {
            var index = new SearchIndex(
                "tags",
                IndexSchema.Create(
                    new IndexSettings
                    {
                        Fields = [new FieldSettings { Name = "tags", LengthNormalization = 0 }],
                    }
                )
            );
            index.Replace([
                new SearchDocument
                {
                    Id = "one",
                    Fields = new() { ["tags"] = ["Winter"] },
                },
                new SearchDocument
                {
                    Id = "five",
                    Fields = new() { ["tags"] = ["Winter", "Sport", "Party", "Work", "Vacation"] },
                },
            ]);

            var hits = index.Search("winter", null, false).Hits;

            Assert.Equal(hits[0].Score, hits[1].Score, 9);
        }

        [Fact]
        public void Search_TermInEveryDocument_ScoresAboveZero()
        {
            var index = Clothing(
                Doc("1", color: "Red"),
                Doc("2", color: "Red"),
                Doc("3", color: "Red")
            );

            var result = index.Search("red", null, false);

            Assert.Equal(3, result.Total);
            Assert.All(result.Hits, hit => Assert.True(hit.Score > 0));
        }

        [Fact]
        public void Search_WithLimit_ReturnsTopHitsOrderedByIdAndCountsAll()
        {
            var index = Clothing(
                Enumerable.Range(0, 15).Select(i => Doc($"{i:D2}", color: "Blue")).ToArray()
            );

            var limited = index.Search("blue", 10, false);

            Assert.Equal(15, limited.Total);
            Assert.Equal(
                Enumerable.Range(0, 10).Select(i => $"{i:D2}"),
                limited.Hits.Select(hit => hit.Id)
            );
            Assert.Equal(15, index.Search("blue", null, false).Hits.Count);
        }

        [Fact]
        public void Search_NegativeLimit_Throws()
        {
            var index = Clothing(Doc("1", color: "Blue"));

            Assert.Throws<InvalidInputException>(() => index.Search("blue", -1, false));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("and the")]
        public void Search_WithoutTerms_FindsNothing(string query)
        {
            var index = Clothing(Doc("1", category: "Jacket"));

            var result = index.Search(query, null, false);

            Assert.Equal(0, result.Total);
            Assert.Empty(result.Hits);
        }

        [Fact]
        public void Search_FieldWithSeveralValues_FindsEveryValue()
        {
            var index = Clothing(
                new SearchDocument
                {
                    Id = "1",
                    Fields = new() { ["tags"] = ["Winter", "Sport"] },
                }
            );

            Assert.Equal(new[] { "1" }, index.SearchIds("sport"));
            Assert.Equal(new[] { "1" }, index.SearchIds("winter sport"));
        }

        [Fact]
        public void Search_WithExplain_DescribesHowEachTermMatched()
        {
            var index = Clothing(Doc("1", category: "Raincoat", color: "Red"));

            var hit = Assert.Single(index.Search("red coat", null, true).Hits);

            Assert.NotNull(hit.Explain);
            Assert.Collection(
                hit.Explain,
                red =>
                {
                    Assert.Equal("red", red.Token);
                    Assert.Equal(MatchKind.Exact, red.Kind);
                    Assert.Equal(new[] { "color" }, red.Fields.Keys);
                },
                coat =>
                {
                    Assert.Equal("raincoat", coat.Token);
                    Assert.Equal(MatchKind.Infix, coat.Kind);
                    Assert.Equal(new[] { "category" }, coat.Fields.Keys);
                }
            );
            Assert.Equal(hit.Explain.Sum(match => match.Score), hit.Score, 6);
        }

        [Fact]
        public void Search_WithoutExplain_OmitsExplanation()
        {
            var index = Clothing(Doc("1", color: "Red"));

            Assert.Null(Assert.Single(index.Search("red", null, false).Hits).Explain);
        }
    }
}
