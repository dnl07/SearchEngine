using Microsoft.AspNetCore.Mvc;
using SearchEngine.Api.Dto.Engine;
using SearchEngine.Api.Mappers.Status;
using SearchEngine.Core.Options;
using SearchEngine.Core.Search;

namespace SearchEngine.Api.Controllers
{
    [ApiController]
    [Route("engine")]
    public class EngineController : ControllerBase
    {
        private readonly Engine _searchEngine;
        private readonly ILogger<EngineController> _logger;

        public EngineController(Engine searchEngine, ILogger<EngineController> logger)
        {
            _searchEngine = searchEngine;
            _logger = logger;
        }

        [HttpPost("init")]
        public IActionResult Add([FromBody] IndexOptions options)
        {
            _searchEngine.Initialize(options);

            _logger.LogInformation(
                "Search engine initialized with options: {options}",
                options.UseOwnIds
            );

            return Ok(new { status = "options changed" });
        }

        [HttpGet("status")]
        public ActionResult<StatusDto> GetStatus()
        {
            _logger.LogInformation("Status called");
            return _searchEngine.GetStatus().ToDto();
        }
    }
}
