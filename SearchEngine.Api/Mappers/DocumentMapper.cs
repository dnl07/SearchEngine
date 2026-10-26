using SearchEngine.Api.Dto.Documents;
using SearchEngine.Core.Exceptions;
using SearchEngine.Models.Documents;

namespace SearchEngine.Api.Mappers.Documents
{
    public static class DocumentMapper
    {
        public static List<SearchDocument> ToModels(this DocumentDto[] dtos) =>
            dtos.Select(dto =>
                    dto is null
                        ? throw new InvalidInputException("Documents must not be null.")
                        : new SearchDocument { Id = dto.Id, Fields = dto.Fields ?? new() }
                )
                .ToList();
    }
}
