namespace SearchEngine.Api.Dto.Search
{
    public class BoostOptionsDto
    {
        public float Title { get; set; } = 3;
        public float Description { get; set; } = 1;
        public float Tags { get; set; } = 2;
    }
}
