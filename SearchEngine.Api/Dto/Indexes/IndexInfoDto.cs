namespace SearchEngine.Api.Dto.Indexes
{
    public class IndexInfoDto
    {
        public string Name { get; set; } = "";
        public IndexSettingsDto Settings { get; set; } = new();
        public int DocumentCount { get; set; }
    }
}
