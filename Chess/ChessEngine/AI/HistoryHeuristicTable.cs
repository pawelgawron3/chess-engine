using ChessEngine.Chessboard;

namespace ChessEngine.AI;

public static class HistoryHeuristicTable
{
    private static int[,] _table = new int[64, 64];

    public static void Add(Move move, int depth)
    {
        _table[move.From.ToIndex(), move.To.ToIndex()] += depth * depth;
    }

    public static int Get(Move move)
    {
        return _table[move.From.ToIndex(), move.To.ToIndex()];
    }

    public static void Reset()
    {
        Array.Clear(_table, 0, _table.Length);
    }
}