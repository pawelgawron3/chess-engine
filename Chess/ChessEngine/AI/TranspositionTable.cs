using ChessEngine.Chessboard;

namespace ChessEngine.AI;

public static class TranspositionTable
{
    public const int Size = 1 << 22; // 4_194_304
    public static readonly TTEntry[] Table = new TTEntry[Size];

    public static ref TTEntry Probe(ulong hash)
    {
        return ref Table[hash & (Size - 1)];
    }

    public static void Store(ulong hash, int depth, int score, BoundType bound, Move? best)
    {
        ref TTEntry entry = ref Probe(hash);

        if (entry.Hash != hash || depth >= entry.Depth)
            entry = new TTEntry(hash, depth, score, bound, best);
    }
}