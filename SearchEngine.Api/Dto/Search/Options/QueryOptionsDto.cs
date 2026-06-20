namespace SearchEngine.Api.Dto.Search
{
    public class QueryOptionsDto
    {
        public int Limit { get; set; } = 10;
        public FuzzyOptionsDto Fuzzy { get; set; } = new FuzzyOptionsDto();
        public ScoreOptionsDto Score { get; set; } = new ScoreOptionsDto();
        public bool Explain { get; set; } = false;
    }
}
