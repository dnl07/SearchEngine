using SearchEngine.Api.Dto.Indexes;
using SearchEngine.Core.Indexes;
using SearchEngine.Models.Indexes;

namespace SearchEngine.Api.Mappers.Indexes
{
    public static class IndexMapper
    {
        /// <summary>
        /// Maps the request settings to the model and replaces missing values with empty defaults.
        /// </summary>
        public static IndexSettings ToModel(this IndexSettingsDto dto) =>
            new()
            {
                Fields = (dto.Fields ?? [])
                    .Select(field => new FieldSettings
                    {
                        Name = field?.Name ?? "",
                        Weight = field?.Weight ?? 0,
                        LengthNormalization = field?.LengthNormalization,
                    })
                    .ToList(),
                Stopwords = new StopwordSettings
                {
                    Languages = dto.Stopwords?.Languages ?? [],
                    Exclude = dto.Stopwords?.Exclude ?? [],
                },
            };

        /// <summary>
        /// Maps the index to its settings and document count.
        /// </summary>
        public static IndexInfoDto ToDto(this SearchIndex index)
        {
            var settings = index.Settings;

            return new IndexInfoDto
            {
                Name = index.Name,
                Settings = new IndexSettingsDto
                {
                    Fields = settings
                        .Fields.Select(field => new FieldSettingsDto
                        {
                            Name = field.Name,
                            Weight = field.Weight,
                            LengthNormalization = field.LengthNormalization,
                        })
                        .ToList(),
                    Stopwords = new StopwordSettingsDto
                    {
                        Languages = settings.Stopwords.Languages.ToList(),
                        Exclude = settings.Stopwords.Exclude.ToList(),
                    },
                },
                DocumentCount = index.DocumentCount,
            };
        }
    }
}
