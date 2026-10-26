using Microsoft.AspNetCore.Mvc;
using SearchEngine.Api.Dto.Indexes;
using SearchEngine.Api.Mappers.Indexes;
using SearchEngine.Core.Exceptions;
using SearchEngine.Core.Indexes;

namespace SearchEngine.Api.Controllers
{
    [ApiController]
    [Route("indexes")]
    public class IndexesController : ControllerBase
    {
        private readonly IndexRegistry _registry;
        private readonly ILogger<IndexesController> _logger;

        public IndexesController(IndexRegistry registry, ILogger<IndexesController> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        [HttpPut("{index}")]
        public ActionResult<IndexInfoDto> CreateOrUpdate(
            string index,
            [FromBody] IndexSettingsDto settings
        )
        {
            var (searchIndex, change) = _registry.CreateOrUpdate(index, settings.ToModel());
            _logger.LogInformation("Index {Index} configured: {Change}", index, change);

            var info = searchIndex.ToDto();

            return change == IndexChange.Created
                ? CreatedAtAction(nameof(Get), new { index }, info)
                : Ok(info);
        }

        [HttpGet("{index}")]
        public ActionResult<IndexInfoDto> Get(string index) => _registry.Get(index).ToDto();

        [HttpDelete("{index}")]
        public IActionResult Delete(string index)
        {
            if (!_registry.Delete(index))
                throw new IndexNotFoundException(index);

            _logger.LogInformation("Index {Index} deleted", index);
            return NoContent();
        }
    }
}
