using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SearchEngine.Api.Dto.Documents;
using SearchEngine.Api.Mappers.Documents;
using SearchEngine.Core.Indexes;

namespace SearchEngine.Api.Controllers
{
    [ApiController]
    [Route("indexes/{index}/documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly IndexRegistry _registry;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IndexRegistry registry, ILogger<DocumentsController> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        [HttpPut]
        public ActionResult<DocumentsWrittenDto> Replace(
            string index,
            [FromBody] DocumentDto[] documents
        )
        {
            var searchIndex = _registry.Get(index);
            var watch = Stopwatch.StartNew();
            int count = searchIndex.Replace(documents.ToModels());
            watch.Stop();

            _logger.LogInformation(
                "Index {Index} replaced with {Count} documents in {Ms}ms",
                index,
                count,
                watch.ElapsedMilliseconds
            );

            return new DocumentsWrittenDto
            {
                Written = documents.Length,
                DocumentCount = count,
                TookMs = watch.ElapsedMilliseconds,
            };
        }

        [HttpPost]
        public ActionResult<DocumentsWrittenDto> Upsert(
            string index,
            [FromBody] DocumentDto[] documents
        )
        {
            var searchIndex = _registry.Get(index);
            var watch = Stopwatch.StartNew();
            int count = searchIndex.Upsert(documents.ToModels());
            watch.Stop();

            _logger.LogInformation(
                "{Written} documents upserted into {Index} in {Ms}ms",
                documents.Length,
                index,
                watch.ElapsedMilliseconds
            );

            return new DocumentsWrittenDto
            {
                Written = documents.Length,
                DocumentCount = count,
                TookMs = watch.ElapsedMilliseconds,
            };
        }

        [HttpDelete("{id}")]
        public IActionResult Remove(string index, string id)
        {
            if (_registry.Get(index).Remove(id))
            {
                _logger.LogInformation("Document {Id} removed from {Index}", id, index);
            }

            return NoContent();
        }
    }
}
