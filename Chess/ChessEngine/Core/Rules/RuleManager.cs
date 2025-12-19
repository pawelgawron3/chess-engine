using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Players;

namespace ChessEngine.Core.Rules;

public class RuleManager
{
    public int? EnPassantFile { get; set; }

    public CastlingRights CastlingRights { get; set; }

    public RuleManager(CastlingRights initialRights)
    {
        CastlingRights = initialRights;
    }

    public void UpdateCastlingRights(Piece movedPiece, Move move)
    {
        if (movedPiece.Type == PieceType.King)
        {
            if (movedPiece.Owner == Player.White)
                CastlingRights &= ~(CastlingRights.WhiteKing | CastlingRights.WhiteQueen);
            else
                CastlingRights &= ~(CastlingRights.BlackKing | CastlingRights.BlackQueen);
        }
        else if (movedPiece.Type == PieceType.Rook)
        {
            if (movedPiece.Owner == Player.White)
            {
                if (move.From.Column == 0)
                    CastlingRights &= ~CastlingRights.WhiteQueen;
                else if (move.From.Column == 7)
                    CastlingRights &= ~CastlingRights.WhiteKing;
            }
            else
            {
                if (move.From.Column == 0)
                    CastlingRights &= ~CastlingRights.BlackQueen;
                else if (move.From.Column == 7)
                    CastlingRights &= ~CastlingRights.BlackKing;
            }
        }
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