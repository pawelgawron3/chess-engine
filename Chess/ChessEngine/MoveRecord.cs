namespace ChessEngine;
public record MoveRecord(
    Move Move,
    Piece MovedPiece,
    Piece? CapturedPiece,
    int HalfMoveClockBefore
);