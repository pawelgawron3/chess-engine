using ChessEngine.Core.Moves;

namespace ChessEngine.Search.Transposition;

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
    public Move? BestMove;

    public TTEntry(ulong hash, int depth, int score, BoundType bound, Move? bestMove)
    {
        Hash = hash;
        Depth = depth;
        Score = score;
        Bound = bound;
        BestMove = bestMove;
    }
}