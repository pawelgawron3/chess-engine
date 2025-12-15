using ChessEngine.Core.Chessboard;

namespace ChessEngine.Core.Moves;

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
public readonly struct Move : IEquatable<Move>
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

    public bool Equals(Move other) =>
        From.Equals(other.From) &&
        To.Equals(other.To) &&
        Type == other.Type &&
        PromotionPiece == other.PromotionPiece;

    public override bool Equals(object? obj) => obj is Move m && Equals(m);

    public override int GetHashCode() => HashCode.Combine(From, To, Type, PromotionPiece);

    public static bool operator ==(Move left, Move right) => left.Equals(right);

    public static bool operator !=(Move left, Move right) => !left.Equals(right);
}