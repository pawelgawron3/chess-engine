using ChessEngine.Chessboard;

namespace ChessEngine.AI;

public static class KillerMoves
{
    private static Move?[,] _table = new Move?[0, 0];
    private const int _maxKillerMoves = 2;
    private static int _maxDepth;

    public static void Init(int depth)
    {
        _maxDepth = depth;
        _table = new Move?[depth + 1, _maxKillerMoves];
    }

    public static void AddKillerMove(int depth, Move move)
    {
        if (depth < 0 || depth > _maxDepth)
            return;

        if (_table[depth, 0] != move)
        {
            _table[depth, 1] = _table[depth, 0];
            _table[depth, 0] = move;
        }
    }

    public static Move? Get(int depth, int index)
    {
        if (depth < 0 || depth > _maxDepth)
            return null;

        return _table[depth, index];
    }
}