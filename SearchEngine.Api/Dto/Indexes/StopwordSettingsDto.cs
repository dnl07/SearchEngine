namespace SearchEngine.Api.Dto.Indexes
{
    public class StopwordSettingsDto
    {
        public List<string> Languages { get; set; } = new();
        public List<string> Exclude { get; set; } = new();
    }
}
