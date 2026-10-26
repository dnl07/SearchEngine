using SearchEngine.Models.Search;

namespace SearchEngine.Core.Matching
{
    public readonly record struct Candidate(
        string Token,
        MatchKind Kind,
        double Boost,
        int Typos = 0
    );
}
