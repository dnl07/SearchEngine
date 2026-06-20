namespace SearchEngine.Api.Dto.Search
{
    public class ScoreOptionsDto
    {
        public double K { get; set; } = 1.75;
        public double B { get; set; } = 0.75;
        public BoostOptionsDto Boost { get; set; } = new BoostOptionsDto();
    }
}
