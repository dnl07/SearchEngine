namespace SearchEngine.Api.Dto.Search
{
    public class FuzzyOptionsDto
    {
        public int MaxEditDistance { get; set; } = 2;
        public int MaxFuzzyExpansions { get; set; } = 5;
        public float ExactMatchBoost { get; set; } = 5;
    }
}
