namespace ChessEngine.Chessboard;
public record MoveRecord(
    Move Move,
    Piece MovedPiece,
    Piece? CapturedPiece,
    int HalfMoveClockBefore,
    int FullMoveCounterBefore,
    int HalfMoveClockAfter,
    int FullMoveCounterAfter,
    ulong PreviousHash,
    ulong HashAfter,
    CastlingRights CastlingRightsBefore,
    CastlingRights CastlingRightsAfter,
    PieceType? PromotedPieceType = null,
    bool KingInCheck = false,
    int? EnPassantFileBefore = null,
    int? EnPassantFileAfter = null
);