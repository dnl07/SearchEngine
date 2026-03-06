using SearchEngine.Api.Dto.Documents;
using SearchEngine.Core.Documents;

namespace SearchEngine.Api.Mappers.Document {
    public static class DocumentMapper {
        public static SearchDocument ToEngineModel(this DocumentRequestDto request) {
            return new SearchDocument {
                Id = request.Id,
                Title = request.Title,
                Description = request.Description,
                Tags = request.Tags
            };
        }
        public static DocumentDto ToDto(this SearchDocument doc) {
            return new DocumentDto {
                Id = doc.Id,
                Title = doc.Title
            };
        }
    }
}