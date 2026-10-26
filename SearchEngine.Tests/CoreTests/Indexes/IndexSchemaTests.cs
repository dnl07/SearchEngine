using SearchEngine.Core.Exceptions;
using SearchEngine.Core.Indexes;
using SearchEngine.Models.Documents;
using SearchEngine.Models.Indexes;
using static SearchEngine.Tests.TestIndexes;

namespace SearchEngine.Tests.CoreTests.Indexes
{
    public class IndexSchemaTests
    {
        [Fact]
        public void Create_WithoutFields_Throws()
        {
            Assert.Throws<InvalidInputException>(() => IndexSchema.Create(new IndexSettings()));
        }

        [Fact]
        public void Create_DuplicateField_Throws()
        {
            var settings = ClothingSettings();
            settings.Fields.Add(new FieldSettings { Name = "tags" });

            Assert.Throws<InvalidInputException>(() => IndexSchema.Create(settings));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        public void Create_InvalidWeight_Throws(double weight)
        {
            var settings = new IndexSettings
            {
                Fields = [new FieldSettings { Name = "title", Weight = weight }],
            };

            Assert.Throws<InvalidInputException>(() => IndexSchema.Create(settings));
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(double.NaN)]
        public void Create_InvalidLengthNormalization_Throws(double lengthNormalization)
        {
            var settings = new IndexSettings
            {
                Fields =
                [
                    new FieldSettings { Name = "title", LengthNormalization = lengthNormalization },
                ],
            };

            Assert.Throws<InvalidInputException>(() => IndexSchema.Create(settings));
        }

        [Fact]
        public void Create_WithoutLengthNormalization_UsesTheBm25Default()
        {
            var schema = IndexSchema.Create(
                new IndexSettings { Fields = [new FieldSettings { Name = "title" }] }
            );

            Assert.Equal(0.75, Assert.Single(schema.Settings.Fields).LengthNormalization);
            Assert.Equal(new[] { 0.75 }, schema.LengthNormalizations);
        }

        [Fact]
        public void Create_UnsupportedLanguage_Throws()
        {
            var settings = ClothingSettings();
            settings.Stopwords.Languages.Add("fr");

            Assert.Throws<InvalidInputException>(() => IndexSchema.Create(settings));
        }

        [Fact]
        public void Create_NormalizesLanguages()
        {
            var settings = ClothingSettings();
            settings.Stopwords.Languages = [" DE ", "de", "en"];

            var schema = IndexSchema.Create(settings);

            Assert.Equal(new[] { "de", "en" }, schema.Settings.Stopwords.Languages);
        }

        [Fact]
        public void Analyze_UnknownField_Throws()
        {
            var schema = IndexSchema.Create(ClothingSettings());
            var doc = new SearchDocument
            {
                Id = "1",
                Fields = new() { ["title"] = ["Jacket"] },
            };

            Assert.Throws<InvalidInputException>(() => schema.Analyze(doc));
        }

        [Fact]
        public void Analyze_WithoutId_Throws()
        {
            var schema = IndexSchema.Create(ClothingSettings());

            Assert.Throws<InvalidInputException>(() => schema.Analyze(Doc(" ", color: "Red")));
        }

        [Fact]
        public void Analyze_PutsTokensAtTheFieldOrdinal()
        {
            var schema = IndexSchema.Create(ClothingSettings());

            var doc = schema.Analyze(Doc("1", category: "T-Shirt", color: "Red and Blue"));

            Assert.Equal(new[] { "t", "shirt", "tshirt" }, doc.Tokens[1]);
            Assert.Equal(new[] { "red", "blue" }, doc.Tokens[3]);
            Assert.Empty(doc.Tokens[0]);
        }

        [Fact]
        public void IsEquivalentTo_ComparesFieldsWeightsAndStopwords()
        {
            var schema = IndexSchema.Create(ClothingSettings());
            var differentWeight = ClothingSettings();
            differentWeight.Fields[0].Weight = 4;
            var differentStopwords = ClothingSettings();
            differentStopwords.Stopwords.Exclude.Add("and");
            var differentNormalization = ClothingSettings();
            differentNormalization.Fields[0].LengthNormalization = 0;

            Assert.True(schema.IsEquivalentTo(IndexSchema.Create(ClothingSettings())));
            Assert.False(schema.IsEquivalentTo(IndexSchema.Create(differentWeight)));
            Assert.False(schema.IsEquivalentTo(IndexSchema.Create(differentStopwords)));
            Assert.False(schema.IsEquivalentTo(IndexSchema.Create(differentNormalization)));
        }
    }
}
