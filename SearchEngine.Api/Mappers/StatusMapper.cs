using SearchEngine.Api.Dto.Engine;
using SearchEngine.Models.Search;

namespace SearchEngine.Api.Mappers.Status {
    public static class StatusMapper {
        public static StatusDto ToDto(this SearchStatus status) {
            return new StatusDto {
                IsRunning = status.IsRunning,
                IsInitialized = status.IsInitialized,
                StartetAt = status.StartetAt,
                TotalDocuments = status.TotalDocuments,
                TotalTokens = status.TotalTokens,
                TotalNGrams = status.TotalNGrams
            };
        }
    }
}