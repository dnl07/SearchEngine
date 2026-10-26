using SearchEngine.Api.Dto.Search;
using SearchEngine.Models.Search;

namespace SearchEngine.Api.Mappers.Search
{
    public static class SearchMapper
    {
        /// <summary>
        /// Maps one query of a multi search to the model.
        /// </summary>
        public static IndexQuery ToModel(this MultiSearchQueryDto dto) =>
            new()
            {
                Index = dto.Index,
                Query = dto.Query,
                Limit = dto.Limit,
                Explain = dto.Explain,
            };

        /// <summary>
        /// Maps a search result to the response, including how long the search took.
        /// </summary>
        public static SearchResponseDto ToDto(this SearchResult result, long tookMs) =>
            new()
            {
                Total = result.Total,
                Hits = result.Hits.Select(ToDto).ToArray(),
                TookMs = tookMs,
            };

        /// <summary>
        /// Maps the result of one multi search query to the response.
        /// </summary>
        public static IndexSearchResultDto ToDto(this IndexSearchResult result) =>
            new()
            {
                Index = result.Index,
                Total = result.Result.Total,
                Hits = result.Result.Hits.Select(ToDto).ToArray(),
            };

        /// <summary>
        /// Maps a hit and its explanation to the response.
        /// </summary>
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
