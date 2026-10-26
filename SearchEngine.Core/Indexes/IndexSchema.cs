using SearchEngine.Core.Analysis;
using SearchEngine.Core.Documents;
using SearchEngine.Core.Exceptions;
using SearchEngine.Core.Ranking;
using SearchEngine.Models.Documents;
using SearchEngine.Models.Indexes;

namespace SearchEngine.Core.Indexes
{
    public sealed class IndexSchema
    {
        private readonly Dictionary<string, int> _ordinals;
        private readonly HashSet<string> _stopwords;

        private IndexSchema(IndexSettings settings, HashSet<string> stopwords)
        {
            Settings = settings;
            FieldNames = settings.Fields.Select(field => field.Name).ToArray();
            Weights = settings.Fields.Select(field => field.Weight).ToArray();
            LengthNormalizations = settings
                .Fields.Select(field => field.LengthNormalization ?? BM25.DefaultB)
                .ToArray();
            _ordinals = FieldNames
                .Select((name, ordinal) => (name, ordinal))
                .ToDictionary(field => field.name, field => field.ordinal, StringComparer.Ordinal);
            _stopwords = stopwords;
        }

        public IndexSettings Settings { get; }
        public IReadOnlyList<string> FieldNames { get; }
        public IReadOnlyList<double> Weights { get; }
        public IReadOnlyList<double> LengthNormalizations { get; }
        public IReadOnlySet<string> Stopwords => _stopwords;

        public static IndexSchema Create(IndexSettings settings)
        {
            var fields = settings.Fields;
            ValidateFields(fields);

            var stopwords = settings.Stopwords;

            var languages = stopwords
                .Languages.Where(language => !string.IsNullOrWhiteSpace(language))
                .Select(language => language.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var unsupported = languages.FirstOrDefault(language =>
                !StopwordsProvider.SupportedLanguages.Contains(language)
            );

            if (unsupported is not null)
                throw new InvalidInputException(
                    $"Stopwords for '{unsupported}' are not supported. Supported languages: {string.Join(", ", StopwordsProvider.SupportedLanguages)}."
                );

            var normalized = new IndexSettings
            {
                Fields = fields
                    .Select(field => new FieldSettings
                    {
                        Name = field.Name,
                        Weight = field.Weight,
                        LengthNormalization = field.LengthNormalization ?? BM25.DefaultB,
                    })
                    .ToList(),
                Stopwords = new StopwordSettings
                {
                    Languages = languages,
                    Exclude = stopwords
                        .Exclude.Where(word => !string.IsNullOrWhiteSpace(word))
                        .Distinct()
                        .ToList(),
                },
            };

            return new IndexSchema(normalized, StopwordsProvider.Resolve(normalized.Stopwords));
        }

        private static void ValidateFields(List<FieldSettings> fields)
        {
            if (fields.Count == 0)
                throw new InvalidInputException("An index needs at least one field.");

            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field.Name))
                    throw new InvalidInputException("Field names must not be empty.");

                if (!double.IsFinite(field.Weight) || field.Weight <= 0)
                    throw new InvalidInputException(
                        $"The weight of field '{field.Name}' must be a positive number."
                    );

                if (field.LengthNormalization is < 0 or > 1 or double.NaN)
                    throw new InvalidInputException(
                        $"The length normalization of field '{field.Name}' must be between 0 and 1."
                    );
            }

            var duplicate = fields
                .GroupBy(field => field.Name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate is not null)
                throw new InvalidInputException($"Field '{duplicate.Key}' is declared twice.");
        }

        public IndexedDocument Analyze(SearchDocument doc)
        {
            if (string.IsNullOrWhiteSpace(doc.Id))
                throw new InvalidInputException("Every document needs a non-empty id.");

            var tokens = new string[FieldNames.Count][];
            Array.Fill(tokens, Array.Empty<string>());

            foreach (var (name, values) in doc.Fields)
            {
                if (!_ordinals.TryGetValue(name, out int ordinal))
                    throw new InvalidInputException(
                        $"Document '{doc.Id}' contains the unknown field '{name}'."
                    );

                tokens[ordinal] = (values ?? [])
                    .SelectMany(value => Tokenizer.TokenizeForIndex(value, _stopwords))
                    .ToArray();
            }

            return new IndexedDocument(doc.Id, tokens);
        }

        public List<string> AnalyzeQuery(string? query) =>
            Tokenizer.TokenizeQuery(query, _stopwords);

        public bool IsEquivalentTo(IndexSchema other) =>
            FieldNames.SequenceEqual(other.FieldNames)
            && Weights.SequenceEqual(other.Weights)
            && LengthNormalizations.SequenceEqual(other.LengthNormalizations)
            && _stopwords.SetEquals(other._stopwords);
    }
}
