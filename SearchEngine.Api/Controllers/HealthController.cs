using SearchEngine.Core.Search;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.Api.Controllers {
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase {
        private readonly Engine _searchEngine;
        private readonly ILogger<HealthController> _logger;

        public HealthController(Engine searchEngine, ILogger<HealthController> logger) {
            _searchEngine = searchEngine;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetHealth() {
        _logger.LogInformation("Health called");
        if (_searchEngine.GetStatus().IsRunning) {
            _logger.LogInformation("Engine is running");
            return Ok();
        }
        _logger.LogWarning("Engine is not running");
        return NotFound();
        }
    }
}