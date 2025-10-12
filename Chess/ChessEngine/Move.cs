namespace ChessEngine;

public enum MoveType
{
    Normal,
    EnPassant,
    Castling,
    Promotion
}

/// <summary>
/// Represents a move from one position to another on the chessboard.
/// </summary>
public readonly struct Move
{
    public Position From { get; }
    public Position To { get; }
    public MoveType Type { get; }
    public PieceType? PromotionPiece { get; }

    public Move(Position from, Position to, MoveType type = MoveType.Normal, PieceType? promotionPiece = null)
    {
        From = from;
        To = to;
        Type = type;
        PromotionPiece = promotionPiece;
    }

    public override string ToString() => $"{From} -> {To}";
}