using ChessEngine.Core.Chessboard;

namespace ChessEngine.Core.Moves;

public struct MoveStruct
{
    public Piece MovedPiece;
    public Piece? CapturedPiece;
    public int HalfMoveClockBefore;
    public ulong HashBefore;
    public CastlingRights CastlingRightsBefore;
    public PieceType? PromotedPieceType;
    public int? EnPassantFileBefore;
}