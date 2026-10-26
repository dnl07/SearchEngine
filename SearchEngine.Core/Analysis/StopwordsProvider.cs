using System.Collections.Concurrent;
using SearchEngine.Models.Indexes;

namespace SearchEngine.Core.Analysis
{
    public static class StopwordsProvider
    {
        public static readonly IReadOnlyList<string> SupportedLanguages = ["de", "en"];

        private static readonly ConcurrentDictionary<string, HashSet<string>> Lists = new();

        /// <summary>
        /// Returns the stopwords of the given languages without the excluded words.
        /// </summary>
        public static HashSet<string> Resolve(StopwordSettings settings)
        {
            var stopwords = new HashSet<string>(StringComparer.Ordinal);

            foreach (var language in settings.Languages)
            {
                stopwords.UnionWith(Lists.GetOrAdd(language, Load));
            }

            foreach (var word in settings.Exclude)
            {
                stopwords.Remove(Normalizer.Normalize(word));
            }

            return stopwords;
        }

        /// <summary>
        /// Loads the embedded stopword list of a language.
        /// </summary>
        private static HashSet<string> Load(string language)
        {
            using var stream =
                typeof(StopwordsProvider).Assembly.GetManifestResourceStream(
                    $"SearchEngine.Core.Data.stopwords_{language}.tsv"
                )
                ?? throw new InvalidOperationException(
                    $"Stopwords for language '{language}' not found."
                );

            using var reader = new StreamReader(stream);

            return reader
                .ReadToEnd()
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Normalizer.Normalize)
                .Where(word => word.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
        }
    }
}
