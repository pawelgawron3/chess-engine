namespace ChessEngine.Components;

public class RuleManager
{
    public int? EnPassantFile { get; private set; }

    public Dictionary<Player, (bool KingMoved, bool RookAMoved, bool RookHMoved)> CastlingRights { get; private set; }

    public RuleManager(Dictionary<Player, (bool, bool, bool)> castlingRights)
    {
        CastlingRights = castlingRights;
    }

    public void UpdateCastlingRights(Piece movedPiece, Move move)
    {
        if (movedPiece.Type == PieceType.King)
        {
            var (king_moved, rookA_moved, rookH_moved) = CastlingRights[movedPiece.Owner];
            if (!king_moved) CastlingRights[movedPiece.Owner] = (true, rookA_moved, rookH_moved);
        }
        else if (movedPiece.Type == PieceType.Rook)
        {
            var (king_moved, rookA_moved, rookH_moved) = CastlingRights[movedPiece.Owner];
            if (!rookA_moved && move.From.Column == 0)
            {
                CastlingRights[movedPiece.Owner] = (king_moved, true, rookH_moved);
            }
            else if (!rookH_moved && move.From.Column == 7)
            {
                CastlingRights[movedPiece.Owner] = (king_moved, rookA_moved, true);
            }
        }
    }

    public void UpdateEnPassantFile(Move move, Piece movedPiece)
    {
        EnPassantFile = null;

        if (movedPiece.Type == PieceType.Pawn && Math.Abs(move.From.Row - move.To.Row) == 2)
        {
            EnPassantFile = move.From.Column;
        }
    }
}