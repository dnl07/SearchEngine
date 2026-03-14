using SearchEngine.Api.Dto.Search;
using SearchEngine.Api.Mappers.Options;
using SearchEngine.Api.Mappers.Search;
using SearchEngine.Core.Search;
using SearchEngine.Models.Search;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SearchEngine.Api.Controllers {
    [ApiController]
    [Route("search")]
    public class SearchController : ControllerBase {
        private readonly Engine _searchEngine;
        private readonly ILogger<SearchController> _logger;

        public SearchController(Engine searchEngine, ILogger<SearchController> logger) {
            _searchEngine = searchEngine;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<SearchResponseDto> Search([FromQuery] string query, [FromQuery] int? limit, [FromQuery] bool explain = false) {
            _logger.LogInformation("Search called with query: {Query}, limit: {Limit}, explain: {Explain}", query, limit, explain);
            var watch = new Stopwatch();
            watch.Start();
            SearchResult searchResult = _searchEngine.Search(query, explain, new QueryOptionsDto() { Limit = limit ?? 10 }.ToEngineModel());
            watch.Stop();

            var searchResponse = searchResult.ToDto();
            searchResponse.ElapsedTime = watch.ElapsedMilliseconds;

            _logger.LogInformation("Search completed in {ElapsedMs}ms", watch.ElapsedMilliseconds);
            return Ok(searchResponse);
        }

        [HttpPost]
        public ActionResult<SearchResponseDto> Search([FromBody] SearchRequestDto request) {
            _logger.LogInformation("Search called with query: {Query}", request.Query);
            var watch = new Stopwatch();
            watch.Start();
            SearchResult searchResult = _searchEngine.Search(request.Query, request.Options.Explain, request.Options.ToEngineModel());
            watch.Stop();

            var searchResponse = searchResult.ToDto();
            searchResponse.ElapsedTime = watch.ElapsedMilliseconds;

            _logger.LogInformation("Search completed in {ElapsedMs}ms", watch.ElapsedMilliseconds);
            return Ok(searchResponse);
        }
    }
}
