using SearchEngine.Api.Dto.Search;
using SearchEngine.Models.Search;

namespace SearchEngine.Api.Mappers.Search
{
    public static class SearchMapper
    {
        public static IndexQuery ToModel(this MultiSearchQueryDto dto) =>
            new()
            {
                Index = dto.Index,
                Query = dto.Query,
                Limit = dto.Limit,
                Explain = dto.Explain,
            };

        public static SearchResponseDto ToDto(this SearchResult result, long tookMs) =>
            new()
            {
                Total = result.Total,
                Hits = result.Hits.Select(ToDto).ToArray(),
                TookMs = tookMs,
            };

        public static IndexSearchResultDto ToDto(this IndexSearchResult result) =>
            new()
            {
                Index = result.Index,
                Total = result.Result.Total,
                Hits = result.Result.Hits.Select(ToDto).ToArray(),
            };

        private static SearchHitDto ToDto(SearchHit hit) =>
            new()
            {
                Id = hit.Id,
                Score = hit.Score,
                Explain = hit
                    .Explain?.Select(match => new TermMatchDto
                    {
                        Term = match.Term,
                        Token = match.Token,
                        Kind = match.Kind,
                        Boost = match.Boost,
                        Score = match.Score,
                        Fields = match.Fields,
                    })
                    .ToArray(),
            };
    }
}
