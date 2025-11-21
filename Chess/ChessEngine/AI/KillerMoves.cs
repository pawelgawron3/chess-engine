using ChessEngine.Chessboard;

namespace ChessEngine.AI;

public static class KillerMoves
{
    public const int MaxKillerMoves = 2;
    public static readonly Move?[,] KillerMovesTable = new Move?[6, MaxKillerMoves];

    public static void AddKillerMove(int depth, Move move)
    {
        if (KillerMovesTable[depth, 0] == null)
        {
            KillerMovesTable[depth, 0] = move;
            return;
        }

        if (KillerMovesTable[depth, 1] == null)
        {
            KillerMovesTable[depth, 1] = move;
            return;
        }

        KillerMovesTable[depth, 1] = KillerMovesTable[depth, 0];
        KillerMovesTable[depth, 0] = move;
    }
}