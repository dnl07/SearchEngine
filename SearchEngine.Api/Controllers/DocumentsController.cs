using System.Diagnostics;
using SearchEngine.Api.Dto.Documents;
using SearchEngine.Api.Mappers.Document;
using SearchEngine.Core.Search;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.Api.Controllers {
    [ApiController]
    [Route("documents")]
    public class DocumentsController : ControllerBase {
        private readonly Engine _searchEngine;

        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(Engine searchEngine, ILogger<DocumentsController> logger) {
            _searchEngine = searchEngine;
            _logger = logger;
        }

        [HttpPost("add")]
        public ActionResult<DocumentResponseDto> Add([FromBody] DocumentRequestDto document) {
            var watch = new Stopwatch();
            watch.Start();
            var doc =_searchEngine.AddDocument(document.ToEngineModel());
            watch.Stop();

            _logger.LogInformation("Document {Id} '{Title}' indexed in {Ms}ms", doc.Id, doc.Title, watch.ElapsedMilliseconds);

            var response = new DocumentResponseDto {
                Status = "Successfully added",
                TotalAdded = 1,
                TookMs = watch.ElapsedMilliseconds,
                AddedDocuments = [doc.ToDto()]
            };

            return Ok(response);
        }

        [HttpPost("bulk")]
        public ActionResult<DocumentResponseDto> AddBulk([FromBody] DocumentRequestDto[] documents) {
            var dtos = new List<DocumentDto>();

            var watch = new Stopwatch();
            watch.Start();
            foreach (var doc in documents) {
                var addedDoc = _searchEngine.AddDocument(doc.ToEngineModel());
                dtos.Add(addedDoc.ToDto());
            }
            watch.Stop();

            _logger.LogInformation("Bulk indexed {Count} documents in {Ms}ms", dtos.Count, watch.ElapsedMilliseconds);

            var response = new DocumentResponseDto {
                Status = "Successfully added",
                TotalAdded = dtos.Count,
                TookMs = watch.ElapsedMilliseconds,
                AddedDocuments = dtos.ToArray()
            };

            return Ok(response);
        }

        [HttpPut("update/{id:Guid}")]
        public IActionResult Update(Guid id, [FromBody] DocumentRequestDto doc) {
            _searchEngine.UpdateDocument(id, doc.ToEngineModel());
            _logger.LogInformation("Document {Id} updated", id);
            return Ok();
        }

 
        [HttpDelete("remove/{id:Guid}")]
        public IActionResult Remove(Guid id) {
            _searchEngine.RemoveDocument(id);
            _logger.LogInformation("Document {Id} removed", id);
            return Ok();
        }       
    }
}
