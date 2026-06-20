namespace SearchEngine.Api.Dto.Documents
{
    public class DocumentResponseDto
    {
        public string Status { get; set; } = "";
        public DocumentDto[] AddedDocuments { get; set; } = [];
        public int TotalAdded { get; set; } = 0;
        public long TookMs { get; set; } = 0;
    }
}
