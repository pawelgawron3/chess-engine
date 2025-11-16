namespace ChessEngine.AI;

public enum BoundType
{
    Exact,
    LowerBound,
    UpperBound
}

public struct TTEntry
{
    public ulong Hash;
    public int Depth;
    public int Score;
    public BoundType Bound;

    public TTEntry(ulong hash, int depth, int score, BoundType bound)
    {
        Hash = hash;
        Depth = depth;
        Score = score;
        Bound = bound;
    }
}