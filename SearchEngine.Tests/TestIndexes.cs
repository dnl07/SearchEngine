using SearchEngine.Core.Indexes;
using SearchEngine.Models.Documents;
using SearchEngine.Models.Indexes;

namespace SearchEngine.Tests
{
    public static class TestIndexes
    {
        public static IndexSettings ClothingSettings() =>
            new()
            {
                Fields =
                [
                    new FieldSettings { Name = "tags", Weight = 3 },
                    new FieldSettings { Name = "category", Weight = 3 },
                    new FieldSettings { Name = "brand", Weight = 2.5 },
                    new FieldSettings { Name = "color", Weight = 2 },
                    new FieldSettings { Name = "size", Weight = 1 },
                    new FieldSettings { Name = "notes", Weight = 0.5 },
                ],
                Stopwords = new StopwordSettings
                {
                    Languages = ["de", "en"],
                    Exclude = ["s", "m", "l", "xl"],
                },
            };

        public static SearchIndex Clothing(params SearchDocument[] docs)
        {
            var index = new SearchIndex("clothing", IndexSchema.Create(ClothingSettings()));
            index.Replace(docs);
            return index;
        }

        public static SearchDocument Doc(
            string id,
            string? tags = null,
            string? category = null,
            string? brand = null,
            string? color = null,
            string? size = null,
            string? notes = null
        )
        {
            var fields = new Dictionary<string, string[]>();

            void Add(string name, string? value)
            {
                if (value is not null)
                {
                    fields[name] = [value];
                }
            }

            Add("tags", tags);
            Add("category", category);
            Add("brand", brand);
            Add("color", color);
            Add("size", size);
            Add("notes", notes);

            return new SearchDocument { Id = id, Fields = fields };
        }

        public static string[] SearchIds(this SearchIndex index, string query) =>
            index.Search(query, null, false).Hits.Select(hit => hit.Id).ToArray();
    }
}
