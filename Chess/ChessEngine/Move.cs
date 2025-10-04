namespace ChessEngine;

/// <summary>
/// Represents a move from one position to another on the chessboard.
/// </summary>
public readonly struct Move
{
    public Position From { get; }
    public Position To { get; }

    public Move(Position from, Position to)
    {
        From = from;
        To = to;
    }

    public override string ToString() => $"{From} -> {To}";
}