namespace SearchEngine.Core.Fuzzy
{
    public readonly record struct FuzzyToken(string Token, int EditDistance);
}
