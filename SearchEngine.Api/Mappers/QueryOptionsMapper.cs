using SearchEngine.Api.Dto.Search;
using SearchEngine.Core.Options;

namespace SearchEngine.Api.Mappers.Options
{
    public static class QueryOptionsMapper
    {
        public static QueryOptions ToEngineModel(this QueryOptionsDto options)
        {
            return new QueryOptions
            {
                MaxDocs = options.Limit,
                MaxEditDistance = options.Fuzzy.MaxEditDistance,
                MaxFuzzyExpansions = options.Fuzzy.MaxFuzzyExpansions,
                ExactMatchBoost = options.Fuzzy.ExactMatchBoost,
                BM25K = options.Score.K,
                BM25B = options.Score.B,
                TitleWeight = options.Score.Boost.Title,
                DescriptionWeight = options.Score.Boost.Description,
                TagWeight = options.Score.Boost.Tags,
            };
        }
    }
}
