namespace ChessEngine;
public record MoveRecord(
    Move Move,
    Piece MovedPiece,
    Piece? CapturedPiece,
    int HalfMoveClockBefore,
    ulong PreviousHash,
    PieceType? PromotedPieceType = null,
    bool KingInCheck = false,
    Dictionary<Player, (bool KingMoved, bool RookAMoved, bool RookHMoved)>? CastlingRightsBefore = null,
    int? EnPassantFileBefore = null
);