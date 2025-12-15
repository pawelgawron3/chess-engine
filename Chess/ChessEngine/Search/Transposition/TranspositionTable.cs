using ChessEngine.Core.Moves;

namespace ChessEngine.Search.Transposition;

public static class TranspositionTable
{
    public const int Size = 1 << 22; // 4_194_304
    public static readonly TTEntry[] Table = new TTEntry[Size];

    public static ref TTEntry Probe(ulong hash)
    {
        return ref Table[hash & (Size - 1)];
    }

    public static void Store(ulong hash, int depth, int score, BoundType bound, Move? bestMove)
    {
        ref TTEntry entry = ref Probe(hash);

        if (entry.Hash != hash || depth >= entry.Depth)
            entry = new TTEntry(hash, depth, score, bound, bestMove);
    }
}