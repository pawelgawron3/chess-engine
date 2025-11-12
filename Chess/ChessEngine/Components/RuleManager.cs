using ChessEngine.Chessboard;

namespace ChessEngine.Components;

public class RuleManager
{
    public int? EnPassantFile { get; internal set; }

    public CastlingRights CastlingRights { get; internal set; }

    public RuleManager(CastlingRights initialRights)
    {
        CastlingRights = initialRights;
    }

    public void UpdateCastlingRights(Piece movedPiece, Move move)
    {
        var rights = CastlingRights;

        if (movedPiece.Owner == Player.White)
        {
            if (movedPiece.Type == PieceType.King)
                rights.White.KingMoved = true;
            else if (movedPiece.Type == PieceType.Rook)
            {
                if (move.From.Column == 0)
                    rights.White.RookAMoved = true;
                else if (move.From.Column == 7)
                    rights.White.RookHMoved = true;
            }
        }
        else if (movedPiece.Owner == Player.Black)
        {
            if (movedPiece.Type == PieceType.King)
                rights.Black.KingMoved = true;
            else if (movedPiece.Type == PieceType.Rook)
            {
                if (move.From.Column == 0)
                    rights.Black.RookAMoved = true;
                else if (move.From.Column == 7)
                    rights.Black.RookHMoved = true;
            }
        }

        CastlingRights = rights;
    }

    public void UpdateEnPassantFile(Move move, Piece movedPiece)
    {
        if (movedPiece.Type == PieceType.Pawn && Math.Abs(move.From.Row - move.To.Row) == 2)
        {
            EnPassantFile = move.From.Column;
        }
        else
        {
            EnPassantFile = null;
        }
    }
}