namespace ChessEngine.Evaluation.Tables;

public static class PieceValues
{
    public static readonly int[] Value = new int[6]
    {
        // pawn, knight, bishop, rook, queen, king
        100, 320, 330, 500, 900, 20_000
    };
}