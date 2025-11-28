namespace ChessEngine.Chessboard;

public struct MoveStruct
{
    public CastlingRights CastlingRightsBefore;
    public int? EnPassantFileBefore;
    public int HalfMoveClockBefore;
    public ulong HashBefore;
    public Piece? CapturedPiece;
}