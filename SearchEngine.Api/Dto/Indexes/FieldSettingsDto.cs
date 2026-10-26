namespace SearchEngine.Api.Dto.Indexes
{
    public class FieldSettingsDto
    {
        public string Name { get; set; } = "";
        public double Weight { get; set; } = 1.0;
        public double? LengthNormalization { get; set; }
    }
}
