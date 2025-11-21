using ChessEngine.Chessboard;

namespace ChessEngine.AI;

public static class KillerMoves
{
    public const int MaxKillerMoves = 2;

    public static readonly Move?[] Killer1 = new Move?[8];
    public static readonly Move?[] Killer2 = new Move?[8];

    public static void AddKillerMove(int depth, Move move)
    {
        if (Killer1[depth] == null)
        {
            Killer1[depth] = move;
            return;
        }

        if (Killer2[depth] == null)
        {
            Killer2[depth] = move;
            return;
        }

        Killer2[depth] = Killer1[depth];
        Killer1[depth] = move;
    }
}