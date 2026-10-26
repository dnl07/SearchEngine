namespace SearchEngine.Api.Dto.Indexes
{
    public class IndexSettingsDto
    {
        public List<FieldSettingsDto> Fields { get; set; } = new();
        public StopwordSettingsDto Stopwords { get; set; } = new();
    }
}
