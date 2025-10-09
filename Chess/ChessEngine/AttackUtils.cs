namespace ChessEngine;

public static class AttackUtils
{
    public static Position? GetKingPosition(Board board, Player player)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Piece? piece = board[row, col];
                if (piece != null && piece.Type == PieceType.King && piece.Owner == player)
                {
                    return new Position(row, col);
                }
            }
        }

        return null;
    }
}