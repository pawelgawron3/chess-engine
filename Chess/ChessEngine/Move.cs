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

    public Move(Position from, Position to, MoveType type = MoveType.Normal)
    {
        From = from;
        To = to;
        Type = type;
    }

    public override string ToString() => $"{From} -> {To}";
}