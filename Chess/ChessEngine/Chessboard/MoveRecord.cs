namespace ChessEngine.Chessboard;
public record MoveRecord(
    Move Move,
    Piece MovedPiece,
    Piece? CapturedPiece,
    int HalfMoveClockBefore,
    ulong PreviousHash,
    CastlingRights CastlingRightsBefore,
    PieceType? PromotedPieceType = null,
    bool KingInCheck = false,
    int? EnPassantFileBefore = null
);