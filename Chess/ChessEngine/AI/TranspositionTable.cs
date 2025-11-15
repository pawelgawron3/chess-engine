namespace ChessEngine.AI;

public static class TranspositionTable
{
    public const int Size = 1 << 22; // 4_194_304
    public static readonly TTEntry[] Table = new TTEntry[Size];
}