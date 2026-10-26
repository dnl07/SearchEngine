using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SearchEngine.Api.Dto.Search;
using SearchEngine.Api.Mappers.Search;
using SearchEngine.Core.Indexes;

namespace SearchEngine.Api.Controllers
{
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IndexRegistry _registry;
        private readonly ILogger<SearchController> _logger;

        public SearchController(IndexRegistry registry, ILogger<SearchController> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        [HttpPost("indexes/{index}/search")]
        public ActionResult<SearchResponseDto> Search(
            string index,
            [FromBody] SearchRequestDto request
        )
        {
            var searchIndex = _registry.Get(index);
            var watch = Stopwatch.StartNew();
            var result = searchIndex.Search(request.Query, request.Limit, request.Explain);
            watch.Stop();

            _logger.LogDebug(
                "Search for '{Query}' in {Index} found {Total} documents in {Ms}ms",
                request.Query,
                index,
                result.Total,
                watch.ElapsedMilliseconds
            );

            return result.ToDto(watch.ElapsedMilliseconds);
        }

        [HttpPost("multi-search")]
        public ActionResult<MultiSearchResponseDto> MultiSearch(
            [FromBody] MultiSearchRequestDto request
        )
        {
            var watch = Stopwatch.StartNew();
            var results = _registry.MultiSearch(
                (request.Queries ?? []).Select(query => query.ToModel()).ToList()
            );
            watch.Stop();

            return new MultiSearchResponseDto
            {
                Results = results.Select(result => result.ToDto()).ToArray(),
                TookMs = watch.ElapsedMilliseconds,
            };
        }
    }
}
